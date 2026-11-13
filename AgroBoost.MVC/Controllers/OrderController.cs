using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using AgroBoost.MVC.Data.EfCore;
using AgroBoost.MVC.Models.Domain;
using AgroBoost.MVC.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace AgroBoost.MVC.Controllers;

public class OrderController : Controller
{
    private readonly AgroBoostDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public OrderController(AgroBoostDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: /Order/Checkout
    [HttpGet]
    [Authorize(Roles = "Buyer,Admin")]
    public async Task<IActionResult> Checkout()
    {
        var user = await _userManager.GetUserAsync(User);
        var cartItems = await _context.CartItems
            .Include(c => c.Product)
                .ThenInclude(p => p.Seller)
            .Where(c => c.BuyerId == user!.Id && c.Product.IsActive)
            .ToListAsync();

        if (!cartItems.Any())
        {
            TempData["ErrorMessage"] = "Your cart is empty.";
            return RedirectToAction("Index", "Cart");
        }

        // Group by seller (simplified: assume single seller for now)
        var firstSeller = cartItems.First().Product.Seller;
        var allSameSeller = cartItems.All(c => c.Product.SellerId == firstSeller.Id);

        var model = new CheckoutViewModel
        {
            CartItems = cartItems,
            ShippingAddress = user.Address ?? string.Empty,
            BuyerName = user.FullName,
            BuyerPhone = user.PhoneNumber ?? string.Empty,
            BuyerEmail = user.Email ?? string.Empty,
            PaymentMethod = "Cash on Delivery",
            Notes = string.Empty
        };

        ViewBag.TotalAmount = cartItems.Sum(c => c.Product.BasePrice * c.Quantity);
        ViewBag.SellerName = firstSeller.OrganizationName ?? firstSeller.FullName;

        return View(model);
    }

    // POST: /Order/Checkout
    [HttpPost]
    [Authorize(Roles = "Buyer,Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(CheckoutViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        var cartItems = await _context.CartItems
            .Include(c => c.Product)
                .ThenInclude(p => p.Seller)
            .Where(c => c.BuyerId == user!.Id && c.Product.IsActive)
            .ToListAsync();

        if (!cartItems.Any())
        {
            ModelState.AddModelError("", "Your cart is empty.");
            return View(model);
        }

        // Validate stock
        foreach (var item in cartItems)
        {
            if (item.Quantity > item.Product.StockQty)
            {
                ModelState.AddModelError("", $"Not enough stock for {item.Product.Name}. Available: {item.Product.StockQty}");
                return View(model);
            }
        }

        // Group by seller and create separate orders
        var ordersBySeller = cartItems.GroupBy(c => c.Product.SellerId);
        var createdOrders = new List<Order>();

        foreach (var group in ordersBySeller)
        {
            var seller = group.First().Product.Seller;
            var totalAmount = group.Sum(c => c.Product.BasePrice * c.Quantity);

            var order = new Order
            {
                BuyerId = user.Id,
                SellerId = seller.Id,
                OrderNumber = $"AGB-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}",
                Status = "Pending",
                TotalAmount = totalAmount,
                ShippingAddress = model.ShippingAddress,
                PaymentMethod = model.PaymentMethod,
                PaymentStatus = "Pending",
                OrderDate = DateTime.UtcNow,
                ExpectedDeliveryDate = DateTime.UtcNow.AddDays(7)
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync(); // Save to get Order.Id
            createdOrders.Add(order);

            foreach (var item in group)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.Product.BasePrice,
                    TotalPrice = item.Product.BasePrice * item.Quantity
                };
                _context.OrderItems.Add(orderItem);

                // Update stock
                item.Product.StockQty -= item.Quantity;
                item.Product.UpdatedAt = DateTime.UtcNow;
            }

            // Clear cart items for this seller
            _context.CartItems.RemoveRange(group);
        }

        await _context.SaveChangesAsync();

        // Show real values on the confirmation page instead of placeholders
        TempData["OrderNumbers"] = string.Join(", ", createdOrders.Select(o => o.OrderNumber));
        TempData["OrderTotal"] = createdOrders.Sum(o => o.TotalAmount);
        TempData["PaymentMethod"] = model.PaymentMethod;

        TempData["SuccessMessage"] = "Order placed successfully!";
        return RedirectToAction("Confirmation");
    }

    // GET: /Order/Confirmation
    [HttpGet]
    [Authorize(Roles = "Buyer,Admin")]
    public IActionResult Confirmation()
    {
        ViewBag.OrderNumbers = TempData["OrderNumbers"];
        ViewBag.OrderTotal = TempData["OrderTotal"];
        ViewBag.PaymentMethod = TempData["PaymentMethod"];
        return View();
    }

    // GET: /Order/History
    [HttpGet]
    [Authorize(Roles = "Buyer,Admin")]
    public async Task<IActionResult> History(int page = 1, int pageSize = 10)
    {
        var user = await _userManager.GetUserAsync(User);
        var query = _context.Orders
            .Include(o => o.Seller)
            .Include(o => o.Items)
                .ThenInclude(oi => oi.Product)
            .Where(o => o.BuyerId == user!.Id)
            .OrderByDescending(o => o.OrderDate);

        var totalOrders = await query.CountAsync();
        var orders = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling((double)totalOrders / pageSize);
        ViewBag.TotalOrders = totalOrders;

        return View(orders);
    }

    // GET: /Order/Details/5
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Details(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        var order = await _context.Orders
            .Include(o => o.Buyer)
            .Include(o => o.Seller)
            .Include(o => o.Items)
                .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == id && (o.BuyerId == user!.Id || o.SellerId == user.Id || User.IsInRole("Admin")));

        if (order == null)
            return NotFound();

        return View(order);
    }
}