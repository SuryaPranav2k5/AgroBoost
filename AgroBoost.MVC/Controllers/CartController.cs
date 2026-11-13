using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AgroBoost.MVC.Data.EfCore;
using AgroBoost.MVC.Models.Domain;

namespace AgroBoost.MVC.Controllers;

public class CartController : Controller
{
    private const string GuestCartSessionKey = "GuestCart";

    private readonly AgroBoostDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public CartController(AgroBoostDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: /Cart
    public async Task<IActionResult> Index()
    {
        var cartItems = await GetCartItemsAsync();
        return View(cartItems);
    }

    // POST: /Cart/Add/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int productId, int quantity = 1)
    {
        if (quantity < 1)
            quantity = 1;

        var product = await _context.Products.FindAsync(productId);
        if (product == null || !product.IsActive || product.StockQty <= 0)
        {
            TempData["ErrorMessage"] = "Product not available.";
            return RedirectToAction("Index", "Home");
        }

        // Guests have no AspNetUsers row, so their cart lives in the session
        // (CartItems.BuyerId is a required FK to AspNetUsers and cannot hold a guest id).
        var userId = GetUserId();
        if (userId == null)
            return AddToGuestCart(product, quantity);

        var existingItem = await _context.CartItems
            .FirstOrDefaultAsync(c => c.BuyerId == userId && c.ProductId == productId);

        if (existingItem != null)
        {
            existingItem.Quantity = Math.Min(existingItem.Quantity + quantity, product.StockQty);
        }
        else
        {
            _context.CartItems.Add(new CartItem
            {
                BuyerId = userId,
                ProductId = productId,
                Quantity = Math.Min(quantity, product.StockQty),
                AddedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Product added to cart.";
        return RedirectToAction("Index");
    }

    // POST: /Cart/Update/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int productId, int quantity)
    {
        var user = await GetCurrentUserAsync();
        if (user == null)
        {
            var guestCart = ReadGuestCart();
            var guestEntry = guestCart.FirstOrDefault(e => e.ProductId == productId);
            if (guestEntry != null)
            {
                if (quantity <= 0)
                    guestCart.Remove(guestEntry);
                else
                    guestEntry.Quantity = quantity;

                WriteGuestCart(guestCart);
            }
            return RedirectToAction("Index");
        }

        var cartItem = await _context.CartItems
            .FirstOrDefaultAsync(c => c.BuyerId == user.Id && c.ProductId == productId);

        if (cartItem == null)
            return NotFound();

        var product = await _context.Products.FindAsync(productId);

        if (quantity <= 0)
        {
            _context.CartItems.Remove(cartItem);
        }
        else
        {
            var maxQty = product?.StockQty ?? quantity;
            cartItem.Quantity = Math.Min(quantity, maxQty);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction("Index");
    }

    // POST: /Cart/Remove/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int productId)
    {
        var user = await GetCurrentUserAsync();
        if (user == null)
        {
            var guestCart = ReadGuestCart();
            guestCart.RemoveAll(e => e.ProductId == productId);
            WriteGuestCart(guestCart);
            return RedirectToAction("Index");
        }

        var cartItem = await _context.CartItems
            .FirstOrDefaultAsync(c => c.BuyerId == user.Id && c.ProductId == productId);

        if (cartItem != null)
        {
            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Index");
    }

    // POST: /Cart/Clear
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Clear()
    {
        var user = await GetCurrentUserAsync();
        if (user == null)
        {
            HttpContext.Session.Remove(GuestCartSessionKey);
            TempData["SuccessMessage"] = "Cart cleared.";
            return RedirectToAction("Index");
        }

        var cartItems = await _context.CartItems
            .Where(c => c.BuyerId == user.Id)
            .ToListAsync();

        _context.CartItems.RemoveRange(cartItems);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Cart cleared.";
        return RedirectToAction("Index");
    }

    private async Task<List<CartItem>> GetCartItemsAsync()
    {
        var user = await GetCurrentUserAsync();
        if (user == null)
            return await GetGuestCartItemsAsync();

        return await _context.CartItems
            .Include(c => c.Product)
                .ThenInclude(p => p.Seller)
            .Include(c => c.Product)
                .ThenInclude(p => p.Category)
            .Where(c => c.BuyerId == user.Id && c.Product.IsActive)
            .ToListAsync();
    }

    /// <summary>
    /// Builds (not-yet-persisted) CartItem objects from the guest session cart,
    /// so the Cart view and the guest add/update/remove actions can be shared.
    /// </summary>
    private async Task<List<CartItem>> GetGuestCartItemsAsync()
    {
        var entries = ReadGuestCart();
        if (entries.Count == 0)
            return new List<CartItem>();

        var productIds = entries.Select(e => e.ProductId).ToList();

        // Navigation properties (Seller, Category) are loaded so the Cart view can use them.
        var products = await _context.Products
            .Include(p => p.Seller)
            .Include(p => p.Category)
            .Where(p => productIds.Contains(p.Id) && p.IsActive)
            .ToListAsync();

        return entries
            .Select(e => new { Entry = e, Product = products.FirstOrDefault(p => p.Id == e.ProductId) })
            .Where(x => x.Product != null)
            .Select(x => new CartItem
            {
                BuyerId = string.Empty, // guest - not persisted
                ProductId = x.Entry.ProductId,
                Quantity = x.Entry.Quantity,
                AddedAt = x.Entry.AddedAt,
                Product = x.Product!
            })
            .ToList();
    }

    private IActionResult AddToGuestCart(Product product, int quantity)
    {
        var guestCart = ReadGuestCart();
        var existingEntry = guestCart.FirstOrDefault(e => e.ProductId == product.Id);

        if (existingEntry != null)
            existingEntry.Quantity = Math.Min(existingEntry.Quantity + quantity, product.StockQty);
        else
            guestCart.Add(new GuestCartEntry
            {
                ProductId = product.Id,
                Quantity = Math.Min(quantity, product.StockQty),
                AddedAt = DateTime.UtcNow
            });

        WriteGuestCart(guestCart);
        TempData["SuccessMessage"] = "Product added to cart.";
        return RedirectToAction("Index");
    }

    private List<GuestCartEntry> ReadGuestCart()
    {
        var json = HttpContext.Session.GetString(GuestCartSessionKey);
        if (string.IsNullOrEmpty(json))
            return new List<GuestCartEntry>();

        try
        {
            return JsonSerializer.Deserialize<List<GuestCartEntry>>(json) ?? new List<GuestCartEntry>();
        }
        catch (JsonException)
        {
            return new List<GuestCartEntry>();
        }
    }

    private void WriteGuestCart(List<GuestCartEntry> entries)
    {
        HttpContext.Session.SetString(GuestCartSessionKey, JsonSerializer.Serialize(entries));
    }

    private string? GetUserId() => User.Identity?.IsAuthenticated == true
        ? _userManager.GetUserId(User)
        : null;

    private async Task<ApplicationUser?> GetCurrentUserAsync() => User.Identity?.IsAuthenticated == true
        ? await _userManager.GetUserAsync(User)
        : null;

    private class GuestCartEntry
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public DateTime AddedAt { get; set; }
    }
}
