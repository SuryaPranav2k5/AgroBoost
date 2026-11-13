using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using AgroBoost.MVC.Data.EfCore;
using AgroBoost.MVC.Models.Domain;
using AgroBoost.MVC.Models.DTOs;
using AgroBoost.MVC.Data.AdoNet.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AgroBoost.MVC.Controllers;

[Authorize(Roles = "Seller,Admin")]
public class SellerController : Controller
{
    private readonly AgroBoostDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ISellerAnalyticsRepository _sellerAnalyticsRepo;

    public SellerController(AgroBoostDbContext context, UserManager<ApplicationUser> userManager, ISellerAnalyticsRepository sellerAnalyticsRepo)
    {
        _context = context;
        _userManager = userManager;
        _sellerAnalyticsRepo = sellerAnalyticsRepo;
    }

    // GET: /Seller/Dashboard
    public async Task<IActionResult> Dashboard()
    {
        var user = await _userManager.GetUserAsync(User);
        
        // Get analytics from ADO.NET SP (Unit V)
        var analytics = await _sellerAnalyticsRepo.GetDashboardAsync(user!.Id);

        // Get recent orders (EF Core)
        var recentOrders = await _context.Orders
            .Include(o => o.Buyer)
            .Include(o => o.Items)
            .Where(o => o.SellerId == user.Id)
            .OrderByDescending(o => o.OrderDate)
            .Take(10)
            .ToListAsync();

        ViewBag.Analytics = analytics;
        ViewBag.RecentOrders = recentOrders;
        ViewBag.TotalProducts = await _context.Products.CountAsync(p => p.SellerId == user.Id && p.IsActive);

        return View();
    }

    // GET: /Seller/Products
    public async Task<IActionResult> Products(int page = 1, int pageSize = 10)
    {
        var user = await _userManager.GetUserAsync(User);
        var query = _context.Products
            .Include(p => p.Category)
            .Where(p => p.SellerId == user!.Id)
            .OrderByDescending(p => p.CreatedAt);

        var totalProducts = await query.CountAsync();
        var products = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling((double)totalProducts / pageSize);
        ViewBag.TotalProducts = totalProducts;

        return View(products);
    }

    // GET: /Seller/CreateProduct
    public async Task<IActionResult> CreateProduct()
    {
        ViewBag.Categories = await _context.Categories
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();
        return View();
    }

    // POST: /Seller/CreateProduct
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateProduct(Product product, IFormFile? imageFile)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.GetUserAsync(User);
            product.SellerId = user!.Id;
            product.CreatedAt = DateTime.UtcNow;
            product.UpdatedAt = DateTime.UtcNow;
            product.IsActive = true;

            // Handle image upload
            if (imageFile != null && imageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "products", user.Id);
                Directory.CreateDirectory(uploadsFolder);
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }
                product.Images = $"/uploads/products/{user.Id}/{fileName}";
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Product created successfully.";
            return RedirectToAction("Products");
        }

        ViewBag.Categories = await _context.Categories
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();
        return View(product);
    }

    // GET: /Seller/EditProduct/5
    public async Task<IActionResult> EditProduct(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id && p.SellerId == user!.Id);

        if (product == null)
            return NotFound();

        ViewBag.Categories = await _context.Categories
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();
        return View(product);
    }

    // POST: /Seller/EditProduct/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProduct(int id, Product product, IFormFile? imageFile)
    {
        if (id != product.Id)
            return NotFound();

        var user = await _userManager.GetUserAsync(User);
        var existingProduct = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id && p.SellerId == user!.Id);

        if (existingProduct == null)
            return NotFound();

        if (ModelState.IsValid)
        {
            existingProduct.Name = product.Name;
            existingProduct.Description = product.Description;
            existingProduct.CategoryId = product.CategoryId;
            existingProduct.Unit = product.Unit;
            existingProduct.BasePrice = product.BasePrice;
            existingProduct.MinOrderQty = product.MinOrderQty;
            existingProduct.StockQty = product.StockQty;
            existingProduct.Certifications = product.Certifications;
            existingProduct.ShelfLifeDays = product.ShelfLifeDays;
            existingProduct.IsActive = product.IsActive;
            existingProduct.UpdatedAt = DateTime.UtcNow;

            // Handle image upload
            if (imageFile != null && imageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "products", user.Id);
                Directory.CreateDirectory(uploadsFolder);
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }
                existingProduct.Images = $"/uploads/products/{user.Id}/{fileName}";
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Product updated successfully.";
            return RedirectToAction("Products");
        }

        ViewBag.Categories = await _context.Categories
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();
        return View(existingProduct);
    }

    // POST: /Seller/DeleteProduct/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id && p.SellerId == user!.Id);

        if (product == null)
            return NotFound();

        product.IsActive = false;
        product.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Product deactivated successfully.";
        return RedirectToAction("Products");
    }

    // GET: /Seller/Orders
    public async Task<IActionResult> Orders(string? status, int page = 1, int pageSize = 10)
    {
        var user = await _userManager.GetUserAsync(User);
        var query = _context.Orders
            .Include(o => o.Buyer)
            .Include(o => o.Items)
                .ThenInclude(oi => oi.Product)
            .Where(o => o.SellerId == user!.Id);

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(o => o.Status == status);
        }

        query = query.OrderByDescending(o => o.OrderDate);

        var totalOrders = await query.CountAsync();
        var orders = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling((double)totalOrders / pageSize);
        ViewBag.TotalOrders = totalOrders;
        ViewBag.StatusFilter = status;

        return View(orders);
    }

    // POST: /Seller/UpdateOrderStatus/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateOrderStatus(int id, string status)
    {
        var user = await _userManager.GetUserAsync(User);
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == id && o.SellerId == user!.Id);

        if (order == null)
            return NotFound();

        order.Status = status;
        if (status == "Delivered")
            order.ActualDeliveryDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Order status updated to {status}.";
        return RedirectToAction("Orders");
    }
}