using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgroBoost.MVC.Models.Domain;

public class Product
{
    public int Id { get; set; }

    [Required]
    [MaxLength(450)]
    public string SellerId { get; set; } = string.Empty;

    [ForeignKey(nameof(SellerId))]
    public virtual ApplicationUser Seller { get; set; } = null!;

    [Required]
    public int CategoryId { get; set; }

    [ForeignKey(nameof(CategoryId))]
    public virtual Category Category { get; set; } = null!;

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    [Required]
    [MaxLength(50)]
    public string Unit { get; set; } = "kg"; // kg, liter, piece

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal BasePrice { get; set; }

    public int MinOrderQty { get; set; } = 1;

    public int StockQty { get; set; }

    [MaxLength(2000)]
    public string? Images { get; set; } // JSON array of paths

    [MaxLength(500)]
    public string? Certifications { get; set; } // JSON: ["FSSAI", "Organic"]

    public int? ShelfLifeDays { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
}