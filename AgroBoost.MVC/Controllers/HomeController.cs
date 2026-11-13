using Microsoft.AspNetCore.Mvc;
using AgroBoost.MVC.Data.EfCore;
using AgroBoost.MVC.Models.Domain;
using AgroBoost.MVC.Models.DTOs;
using AgroBoost.MVC.Data.AdoNet.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AgroBoost.MVC.Controllers;

public class HomeController : Controller
{
    private readonly AgroBoostDbContext _context;
    private readonly IProductSearchRepository _productSearchRepo;

    public HomeController(AgroBoostDbContext context, IProductSearchRepository productSearchRepo)
    {
        _context = context;
        _productSearchRepo = productSearchRepo;
    }

    // GET: /Home/Index
    public async Task<IActionResult> Index()
    {
        // ViewBag for categories (Unit III - ViewBag)
        ViewBag.Categories = await _context.Categories
            .Where(c => c.IsActive && c.ParentCategoryId == null)
            .OrderBy(c => c.Name)
            .ToListAsync();

        // Featured products (EF Core)
        var featuredProducts = await _context.Products
            .Where(p => p.IsActive && p.StockQty > 0)
            .Include(p => p.Category)
            .Include(p => p.Seller)
            .OrderByDescending(p => p.CreatedAt)
            .Take(8)
            .ToListAsync();

        ViewData["FeaturedProducts"] = featuredProducts;

        return View();
    }

    // GET: /Home/Search
    public async Task<IActionResult> Search(string? keyword, int? categoryId, decimal? minPrice, decimal? maxPrice, string? sellerType, int page = 1, int pageSize = 12)
    {
        // ViewBag for categories (Unit III - ViewBag)
        ViewBag.Categories = await _context.Categories
            .Where(c => c.IsActive && c.ParentCategoryId == null)
            .OrderBy(c => c.Name)
            .ToListAsync();

        // Use ADO.NET SP for advanced search (Unit V)
        var searchDto = new ProductSearchDto
        {
            Keyword = keyword,
            CategoryId = categoryId,
            MinPrice = minPrice,
            MaxPrice = maxPrice,
            SellerType = sellerType,
            PageNumber = page,
            PageSize = pageSize
        };

        var (products, total) = await _productSearchRepo.SearchAsync(searchDto);

        ViewBag.Keyword = keyword;
        ViewBag.CategoryId = categoryId;
        ViewBag.MinPrice = minPrice;
        ViewBag.MaxPrice = maxPrice;
        ViewBag.SellerType = sellerType;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling((double)total / pageSize);
        ViewBag.TotalCount = total;

        TempData["SearchKeyword"] = keyword;
        TempData["SearchCategoryId"] = categoryId;
        TempData["SearchMinPrice"] = minPrice;
        TempData["SearchMaxPrice"] = maxPrice;
        TempData["SearchSellerType"] = sellerType;
        TempData["SearchPage"] = page;
        TempData["SearchPageSize"] = pageSize;

        return View(products);
    }

    // GET: /Home/Privacy
    public IActionResult Privacy()
    {
        return View();
    }

    // GET: /Home/Error
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new AgroBoost.MVC.Models.ErrorViewModel
        {
            RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }
}