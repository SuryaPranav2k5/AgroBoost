using Microsoft.AspNetCore.Mvc;
using AgroBoost.MVC.Data.EfCore;
using AgroBoost.MVC.Models.Domain;
using AgroBoost.MVC.Models.DTOs;
using AgroBoost.MVC.Data.AdoNet.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace AgroBoost.MVC.Controllers;

public class ProductController : Controller
{
    private readonly AgroBoostDbContext _context;
    private readonly IProductSearchRepository _productSearchRepo;

    public ProductController(AgroBoostDbContext context, IProductSearchRepository productSearchRepo)
    {
        _context = context;
        _productSearchRepo = productSearchRepo;
    }

    // GET: /Product/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Seller)
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

        if (product == null)
            return NotFound();

        return View(product);
    }

    // GET: /Product/Create
    [HttpGet]
    [Authorize(Roles = "Seller,Admin")]
    public async Task<IActionResult> Create()
    {
        ViewBag.Categories = await _context.Categories
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();
        return View();
    }

    // POST: /Product/Create
    [HttpPost]
    [Authorize(Roles = "Seller,Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product product, IFormFile? imageFile)
    {
        if (ModelState.IsValid)
        {
            var user = await _context.Users.FindAsync(User.FindFirst("sub")?.Value ?? User.Identity?.Name);
            if (user == null)
                return Challenge();

            product.SellerId = user.Id;
            product.CreatedAt = DateTime.UtcNow;
            product.UpdatedAt = DateTime.UtcNow;
            product.IsActive = true;

            // Handle image upload
            if (imageFile != null && imageFile.Length > 0)
            {
                // Simple implementation - in production use IFileUploadService
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
            return RedirectToAction("Dashboard", "Seller");
        }

        ViewBag.Categories = await _context.Categories
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();
        return View(product);
    }

    // GET: /Product/Edit/5
    [HttpGet]
    [Authorize(Roles = "Seller,Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        var userId = User.FindFirst("sub")?.Value ?? User.Identity?.Name;
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id && p.SellerId == userId);

        if (product == null)
            return NotFound();

        ViewBag.Categories = await _context.Categories
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();
        return View(product);
    }

    // POST: /Product/Edit/5
    [HttpPost]
    [Authorize(Roles = "Seller,Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Product product, IFormFile? imageFile)
    {
        if (id != product.Id)
            return NotFound();

        var userId = User.FindFirst("sub")?.Value ?? User.Identity?.Name;
        var existingProduct = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id && p.SellerId == userId);

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
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "products", userId);
                Directory.CreateDirectory(uploadsFolder);
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }
                existingProduct.Images = $"/uploads/products/{userId}/{fileName}";
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Product updated successfully.";
            return RedirectToAction("Dashboard", "Seller");
        }

        ViewBag.Categories = await _context.Categories
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();
        return View(existingProduct);
    }

    // POST: /Product/Delete/5
    [HttpPost]
    [Authorize(Roles = "Seller,Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = User.FindFirst("sub")?.Value ?? User.Identity?.Name;
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id && p.SellerId == userId);

        if (product == null)
            return NotFound();

        product.IsActive = false;
        product.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Product deactivated successfully.";
        return RedirectToAction("Dashboard", "Seller");
    }
}