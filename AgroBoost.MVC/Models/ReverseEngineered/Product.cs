using System;
using System.Collections.Generic;

namespace AgroBoost.MVC.Models.ReverseEngineered;

public partial class Product
{
    public int Id { get; set; }

    public string SellerId { get; set; } = null!;

    public int CategoryId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string Unit { get; set; } = null!;

    public decimal BasePrice { get; set; }

    public int MinOrderQty { get; set; }

    public int StockQty { get; set; }

    public string? Images { get; set; }

    public string? Certifications { get; set; }

    public int? ShelfLifeDays { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual AspNetUser Seller { get; set; } = null!;
}
