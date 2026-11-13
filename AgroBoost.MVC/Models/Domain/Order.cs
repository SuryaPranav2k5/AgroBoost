using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgroBoost.MVC.Models.Domain;

public class Order
{
    public int Id { get; set; }

    [Required]
    [MaxLength(450)]
    public string BuyerId { get; set; } = string.Empty;

    [ForeignKey(nameof(BuyerId))]
    public virtual ApplicationUser Buyer { get; set; } = null!;

    [Required]
    [MaxLength(450)]
    public string SellerId { get; set; } = string.Empty;

    [ForeignKey(nameof(SellerId))]
    public virtual ApplicationUser Seller { get; set; } = null!;

    [Required]
    [MaxLength(50)]
    public string OrderNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Pending"; // Pending, Confirmed, Processing, Shipped, Delivered, Cancelled

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    [Required]
    [MaxLength(1000)]
    public string ShippingAddress { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string PaymentMethod { get; set; } = "Cash on Delivery";

    [Required]
    [MaxLength(20)]
    public string PaymentStatus { get; set; } = "Pending"; // Pending, Paid, Failed, Refunded

    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    public DateTime? ExpectedDeliveryDate { get; set; }

    public DateTime? ActualDeliveryDate { get; set; }

    // Navigation properties
    public virtual ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}