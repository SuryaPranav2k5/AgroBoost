using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AgroBoost.MVC.Data.EfCore;
using AgroBoost.MVC.Models.Domain;
using AgroBoost.MVC.Data.Seeding;
using AgroBoost.MVC.Middleware;
using AgroBoost.MVC.Services;
using AgroBoost.MVC.Data.AdoNet.Repository.Interfaces;
using AgroBoost.MVC.Data.AdoNet.Repository.Implementations;

var builder = WebApplication.CreateBuilder(args);

// EF Core (Unit IV)
builder.Services.AddDbContext<AgroBoostDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity (Unit IV)
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false;
})
.AddEntityFrameworkStores<AgroBoostDbContext>()
.AddDefaultTokenProviders();

// Session (Unit III - Guest Cart)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Cookie Authentication (Unit IV) - Configured via AddIdentity above
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
});

// Authorization Policies (Unit IV)
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", policy => policy.RequireRole("Admin"));
    options.AddPolicy("RequireSeller", policy => policy.RequireRole("Seller", "Admin"));
    options.AddPolicy("RequireBuyer", policy => policy.RequireRole("Buyer", "Admin"));
});

// ADO.NET Repositories (Unit V)
builder.Services.AddScoped<IProductSearchRepository, ProductSearchRepository>();
builder.Services.AddScoped<ISellerAnalyticsRepository, SellerAnalyticsRepository>();
builder.Services.AddScoped<IAdminReportRepository, AdminReportRepository>();

// Services
builder.Services.AddScoped<IFileUploadService, FileUploadService>();

// MVC Controllers with Views
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Seed Database
using (var scope = app.Services.CreateScope())
{
    await DbInitializer.InitializeAsync(scope.ServiceProvider);
}

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    // In Development, show the real exception + stack trace in the browser.
    // Must come FIRST so it wraps the custom middleware below.
    app.UseDeveloperExceptionPage();
}

// Custom Middleware Pipeline (Unit IV)
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseSession();                    // Unit III - Session for guest cart
app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<ApiKeyAuthMiddleware>(); // Unit VI - API Key for /api/*

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllers();

app.Run();