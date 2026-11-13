using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using AgroBoost.MVC.Data.EfCore;
using AgroBoost.MVC.Models.Domain;
using AgroBoost.MVC.Models.DTOs;
using AgroBoost.MVC.Data.AdoNet.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AgroBoost.MVC.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly AgroBoostDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAdminReportRepository _adminReportRepo;

    public AdminController(AgroBoostDbContext context, UserManager<ApplicationUser> userManager, IAdminReportRepository adminReportRepo)
    {
        _context = context;
        _userManager = userManager;
        _adminReportRepo = adminReportRepo;
    }

    // GET: /Admin/Dashboard
    public async Task<IActionResult> Dashboard()
    {
        // Get platform report from ADO.NET SP (Unit V)
        var report = await _adminReportRepo.GetPlatformReportAsync();

        ViewBag.Report = report;
        return View();
    }

    // GET: /Admin/Sellers
    public async Task<IActionResult> Sellers(int page = 1, int pageSize = 15, string? status = null)
    {
        var query = _context.Users
            .Where(u => u.UserType == "Seller" && u.IsActive);

        if (status == "verified")
            query = query.Where(u => u.IsVerified);
        else if (status == "pending")
            query = query.Where(u => !u.IsVerified);

        query = query.OrderByDescending(u => u.CreatedAt);

        var totalSellers = await query.CountAsync();
        var sellers = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling((double)totalSellers / pageSize);
        ViewBag.TotalSellers = totalSellers;
        ViewBag.StatusFilter = status;

        return View(sellers);
    }

    // POST: /Admin/VerifySeller
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifySeller(string userId, bool verify)
    {
        var seller = await _userManager.FindByIdAsync(userId);
        if (seller == null || seller.UserType != "Seller")
            return NotFound();

        seller.IsVerified = verify;
        await _userManager.UpdateAsync(seller);

        TempData["SuccessMessage"] = verify ? "Seller verified successfully." : "Seller verification revoked.";
        return RedirectToAction("Sellers");
    }

    // GET: /Admin/Reports
    public async Task<IActionResult> Reports()
    {
        // Get detailed platform report from ADO.NET SP (Unit V)
        var report = await _adminReportRepo.GetPlatformReportAsync();
        return View(report);
    }

    // GET: /Admin/Users
    public async Task<IActionResult> Users(string? userType, int page = 1, int pageSize = 20)
    {
        var query = _context.Users
            .Where(u => u.IsActive);

        if (!string.IsNullOrEmpty(userType))
        {
            query = query.Where(u => u.UserType == userType);
        }

        query = query.OrderByDescending(u => u.CreatedAt);

        var totalUsers = await query.CountAsync();
        var users = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling((double)totalUsers / pageSize);
        ViewBag.TotalUsers = totalUsers;
        ViewBag.UserTypeFilter = userType;

        return View(users);
    }

    // POST: /Admin/ToggleUserStatus
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleUserStatus(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound();

        user.IsActive = !user.IsActive;
        await _userManager.UpdateAsync(user);

        TempData["SuccessMessage"] = user.IsActive ? "User activated." : "User deactivated.";
        return RedirectToAction("Users");
    }
}