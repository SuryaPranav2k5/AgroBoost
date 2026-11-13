using System.ComponentModel.DataAnnotations;
using AgroBoost.MVC.Models.Domain;

namespace AgroBoost.MVC.Models.ViewModels;

public class CheckoutViewModel
{
    public List<CartItem> CartItems { get; set; } = new();

    [Required]
    [MaxLength(1000)]
    [Display(Name = "Shipping Address")]
    public string ShippingAddress { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    [Display(Name = "Your Name")]
    public string BuyerName { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    [Phone]
    [Display(Name = "Phone Number")]
    public string BuyerPhone { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string BuyerEmail { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Payment Method")]
    public string PaymentMethod { get; set; } = "Cash on Delivery";

    [MaxLength(500)]
    [Display(Name = "Order Notes")]
    public string Notes { get; set; } = string.Empty;
}