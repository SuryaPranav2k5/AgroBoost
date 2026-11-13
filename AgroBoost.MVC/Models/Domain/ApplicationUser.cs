using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgroBoost.MVC.Models.Domain;

public class ApplicationUser : IdentityUser
{
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(20)]
    public string UserType { get; set; } = "Buyer"; // "Buyer", "Seller", "Admin"

    [MaxLength(200)]
    public string? OrganizationName { get; set; }

    [MaxLength(20)]
    public string? OrganizationType { get; set; } // "SHG", "FPG", "Company", null

    [MaxLength(500)]
    public string? Address { get; set; }

    [MaxLength(100)]
    public string? District { get; set; }

    [MaxLength(100)]
    public string? State { get; set; }

    [MaxLength(20)]
    public string? GSTNumber { get; set; }

    [MaxLength(50)]
    public string? RegistrationNumber { get; set; }

    public bool IsVerified { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    public virtual ICollection<Order> OrdersAsBuyer { get; set; } = new List<Order>();
    public virtual ICollection<Order> OrdersAsSeller { get; set; } = new List<Order>();
    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
}