using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AgroBoost.MVC.Models.Domain;
using AgroBoost.MVC.Data.EfCore;

namespace AgroBoost.MVC.Data.Seeding;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AgroBoostDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        // Seed Roles
        await SeedRolesAsync(roleManager);

        // Seed Admin User
        await SeedAdminUserAsync(userManager);

        // Seed Sellers and Buyers (mock demo accounts)
        var sellers = await SeedUsersAsync(userManager);

        // Seed Categories
        await SeedCategoriesAsync(context);

        // Seed Products (depends on sellers + categories)
        await SeedProductsAsync(context, sellers);

        // Give any product that has no image a local product illustration
        await BackfillProductImagesAsync(context);
    }

    /// <summary>
    /// Assigns the bundled product artwork to products that have no image.
    /// Idempotent: only fills empty/null Images, so it is safe to run on every startup
    /// and works for databases seeded before these images existed.
    /// </summary>
    private static async Task BackfillProductImagesAsync(AgroBoostDbContext context)
    {
        var imagesByName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Basmati Rice"] = "/images/products/basmati-rice.jpg",
            ["Turmeric Powder"] = "/images/products/turmeric-powder.jpg",
            ["Fresh Apple"] = "/images/products/fresh-apple.jpg",
            ["Wild Forest Honey"] = "/images/products/wild-forest-honey.jpg",
            ["Toor Dal"] = "/images/products/toor-dal.jpg",
            ["Cold-Pressed Coconut Oil"] = "/images/products/coconut-oil.jpg",
        };

        var productsWithoutImage = await context.Products
            .Where(p => p.Images == null || p.Images == "")
            .ToListAsync();

        var updated = false;

        foreach (var product in productsWithoutImage)
        {
            if (imagesByName.TryGetValue(product.Name, out var imagePath))
            {
                product.Images = imagePath;
                product.UpdatedAt = DateTime.UtcNow;
                updated = true;
            }
        }

        if (updated)
        {
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        string[] roles = { "Admin", "Seller", "Buyer" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    private static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager)
    {
        const string adminEmail = "admin@agroboost.com";
        const string adminPassword = "Admin@123";

        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FullName = "System Administrator",
                UserType = "Admin",
                IsVerified = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(admin, adminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Admin");
            }
        }
    }

    private static async Task SeedCategoriesAsync(AgroBoostDbContext context)
    {
        if (await context.Categories.AnyAsync())
            return;

        var categories = new List<Category>
        {
            new Category { Name = "Grains & Cereals", Description = "Rice, wheat, millets, and other grains", IsActive = true },
            new Category { Name = "Pulses & Lentils", Description = "Dal, beans, chickpeas, and other pulses", IsActive = true },
            new Category { Name = "Spices & Condiments", Description = "Turmeric, cumin, coriander, and other spices", IsActive = true },
            new Category { Name = "Fruits & Vegetables", Description = "Fresh and processed fruits and vegetables", IsActive = true },
            new Category { Name = "Dairy Products", Description = "Milk, ghee, paneer, and other dairy items", IsActive = true },
            new Category { Name = "Oilseeds & Oils", Description = "Mustard, sesame, groundnut oils", IsActive = true },
            new Category { Name = "Processed Foods", Description = "Pickles, jams, sauces, and ready-to-eat items", IsActive = true },
            new Category { Name = "Organic Products", Description = "Certified organic agricultural products", IsActive = true },
        };

        await context.Categories.AddRangeAsync(categories);
        await context.SaveChangesAsync();

        // Add sub-categories
        var grains = await context.Categories.FirstAsync(c => c.Name == "Grains & Cereals");
        var pulses = await context.Categories.FirstAsync(c => c.Name == "Pulses & Lentils");
        var spices = await context.Categories.FirstAsync(c => c.Name == "Spices & Condiments");

        var subCategories = new List<Category>
        {
            new Category { Name = "Rice Varieties", Description = "Basmati, Sona Masoori, etc.", ParentCategoryId = grains.Id, IsActive = true },
            new Category { Name = "Wheat Products", Description = "Whole wheat, flour, semolina", ParentCategoryId = grains.Id, IsActive = true },
            new Category { Name = "Millets", Description = "Ragi, Jowar, Bajra, Foxtail", ParentCategoryId = grains.Id, IsActive = true },
            new Category { Name = "Toor Dal", Description = "Split pigeon peas", ParentCategoryId = pulses.Id, IsActive = true },
            new Category { Name = "Moong Dal", Description = "Green gram split", ParentCategoryId = pulses.Id, IsActive = true },
            new Category { Name = "Chana Dal", Description = "Bengal gram split", ParentCategoryId = pulses.Id, IsActive = true },
            new Category { Name = "Whole Spices", Description = "Whole cumin, coriander, mustard", ParentCategoryId = spices.Id, IsActive = true },
            new Category { Name = "Ground Spices", Description = "Turmeric powder, chili powder, garam masala", ParentCategoryId = spices.Id, IsActive = true },
        };

        await context.Categories.AddRangeAsync(subCategories);
        await context.SaveChangesAsync();
    }

    private static async Task<List<ApplicationUser>> SeedUsersAsync(UserManager<ApplicationUser> userManager)
    {
        const string password = "Demo@1234";

        var sellers = new[]
        {
            new ApplicationUser
            {
                UserName = "seller1@agroboost.com",
                Email = "seller1@agroboost.com",
                EmailConfirmed = true,
                FullName = "Green Valley Self Help Group",
                UserType = "Seller",
                OrganizationName = "Green Valley SHG",
                OrganizationType = "SHG",
                Address = "Village Green Valley, Amritsar",
                District = "Amritsar",
                State = "Punjab",
                IsVerified = true,
                IsActive = true,
            },
            new ApplicationUser
            {
                UserName = "seller2@agroboost.com",
                Email = "seller2@agroboost.com",
                EmailConfirmed = true,
                FullName = "Himalayan Farmer Producer Group",
                UserType = "Seller",
                OrganizationName = "Himalayan FPG",
                OrganizationType = "FPG",
                Address = "Kullu Valley",
                District = "Kullu",
                State = "Himachal Pradesh",
                IsVerified = true,
                IsActive = true,
            },
            new ApplicationUser
            {
                UserName = "seller3@agroboost.com",
                Email = "seller3@agroboost.com",
                EmailConfirmed = true,
                FullName = "Organic Farmers Co-operative",
                UserType = "Seller",
                OrganizationName = "Organic Farmers Co-op",
                OrganizationType = "Company",
                Address = "Coorg, Kodagu",
                District = "Kodagu",
                State = "Karnataka",
                IsVerified = true,
                IsActive = true,
            },
        };

        var buyers = new[]
        {
            new ApplicationUser
            {
                UserName = "buyer1@agroboost.com",
                Email = "buyer1@agroboost.com",
                EmailConfirmed = true,
                FullName = "Rajesh Kumar",
                UserType = "Buyer",
                Address = "123 MG Road",
                District = "Bengaluru Urban",
                State = "Karnataka",
                IsVerified = true,
                IsActive = true,
            },
            new ApplicationUser
            {
                UserName = "buyer2@agroboost.com",
                Email = "buyer2@agroboost.com",
                EmailConfirmed = true,
                FullName = "Priya Sharma",
                UserType = "Buyer",
                Address = "45 Park Street",
                District = "Kolkata",
                State = "West Bengal",
                IsVerified = true,
                IsActive = true,
            },
        };

        var createdSellers = new List<ApplicationUser>();

        foreach (var seller in sellers)
        {
            var existing = await userManager.FindByEmailAsync(seller.Email!);
            if (existing == null)
            {
                var result = await userManager.CreateAsync(seller, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(seller, "Seller");
                    createdSellers.Add(seller);
                }
            }
            else
            {
                createdSellers.Add(existing);
            }
        }

        foreach (var buyer in buyers)
        {
            if (await userManager.FindByEmailAsync(buyer.Email!) == null)
            {
                var result = await userManager.CreateAsync(buyer, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(buyer, "Buyer");
                }
            }
        }

        return createdSellers;
    }

    private static async Task SeedProductsAsync(AgroBoostDbContext context, List<ApplicationUser> sellers)
    {
        if (await context.Products.AnyAsync())
            return;

        var seller1 = sellers[0];
        var seller2 = sellers[1];
        var seller3 = sellers[2];

        var riceCat = await context.Categories.FirstAsync(c => c.Name == "Rice Varieties");
        var spicesCat = await context.Categories.FirstAsync(c => c.Name == "Ground Spices");
        var fruitsCat = await context.Categories.FirstAsync(c => c.Name == "Fruits & Vegetables");
        var processedCat = await context.Categories.FirstAsync(c => c.Name == "Processed Foods");
        var pulsesCat = await context.Categories.FirstAsync(c => c.Name == "Toor Dal");
        var oilseedsCat = await context.Categories.FirstAsync(c => c.Name == "Oilseeds & Oils");

        var products = new List<Product>
        {
            new Product
            {
                SellerId = seller1.Id,
                CategoryId = riceCat.Id,
                Name = "Basmati Rice",
                Description = "Premium long-grain basmati rice from Punjab farms",
                Unit = "kg",
                BasePrice = 90m,
                MinOrderQty = 1,
                StockQty = 500,
                IsActive = true,
                Certifications = "[\"FSSAI\"]",
                ShelfLifeDays = 12,
                Images = "/images/products/basmati-rice.jpg",
            },
            new Product
            {
                SellerId = seller1.Id,
                CategoryId = spicesCat.Id,
                Name = "Turmeric Powder",
                Description = "Pure organic turmeric powder with high curcumin content",
                Unit = "kg",
                BasePrice = 220m,
                MinOrderQty = 1,
                StockQty = 150,
                IsActive = true,
                Certifications = "[\"FSSAI\",\"Organic\"]",
                ShelfLifeDays = 18,
                Images = "/images/products/turmeric-powder.jpg",
            },
            new Product
            {
                SellerId = seller2.Id,
                CategoryId = fruitsCat.Id,
                Name = "Fresh Apple",
                Description = "Crisp and juicy Himalayan apples",
                Unit = "kg",
                BasePrice = 120m,
                MinOrderQty = 2,
                StockQty = 300,
                IsActive = true,
                Certifications = "[]",
                ShelfLifeDays = 14,
                Images = "/images/products/fresh-apple.jpg",
            },
            new Product
            {
                SellerId = seller2.Id,
                CategoryId = processedCat.Id,
                Name = "Wild Forest Honey",
                Description = "Raw unprocessed honey from Himalayan forests",
                Unit = "kg",
                BasePrice = 450m,
                MinOrderQty = 1,
                StockQty = 100,
                IsActive = true,
                Certifications = "[\"FSSAI\",\"Organic\"]",
                ShelfLifeDays = 24,
                Images = "/images/products/wild-forest-honey.jpg",
            },
            new Product
            {
                SellerId = seller3.Id,
                CategoryId = pulsesCat.Id,
                Name = "Toor Dal",
                Description = "Split pigeon peas from Karnataka farms",
                Unit = "kg",
                BasePrice = 110m,
                MinOrderQty = 1,
                StockQty = 250,
                IsActive = true,
                Certifications = "[\"Organic\"]",
                ShelfLifeDays = 12,
                Images = "/images/products/toor-dal.jpg",
            },
            new Product
            {
                SellerId = seller3.Id,
                CategoryId = oilseedsCat.Id,
                Name = "Cold-Pressed Coconut Oil",
                Description = "Pure cold-pressed coconut oil",
                Unit = "liter",
                BasePrice = 180m,
                MinOrderQty = 1,
                StockQty = 200,
                IsActive = true,
                Certifications = "[\"FSSAI\",\"Organic\"]",
                ShelfLifeDays = 9,
                Images = "/images/products/coconut-oil.jpg",
            },
        };

        await context.Products.AddRangeAsync(products);
        await context.SaveChangesAsync();
    }
}