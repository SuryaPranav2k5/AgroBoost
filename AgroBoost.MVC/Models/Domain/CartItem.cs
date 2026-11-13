using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgroBoost.MVC.Models.Domain;

public class CartItem
{
    public int Id { get; set; }

    [Required]
    [MaxLength(450)]
    public string BuyerId { get; set; } = string.Empty;

    [ForeignKey(nameof(BuyerId))]
    public virtual ApplicationUser Buyer { get; set; } = null!;

    [Required]
    public int ProductId { get; set; }

    [ForeignKey(nameof(ProductId))]
    public virtual Product Product { get; set; } = null!;

    [Required]
    public int Quantity { get; set; }

    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}