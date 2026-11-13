using System;
using System.Collections.Generic;

namespace AgroBoost.MVC.Models.ReverseEngineered;

public partial class Order
{
    public int Id { get; set; }

    public string BuyerId { get; set; } = null!;

    public string SellerId { get; set; } = null!;

    public string OrderNumber { get; set; } = null!;

    public string Status { get; set; } = null!;

    public decimal TotalAmount { get; set; }

    public string ShippingAddress { get; set; } = null!;

    public string PaymentMethod { get; set; } = null!;

    public string PaymentStatus { get; set; } = null!;

    public DateTime OrderDate { get; set; }

    public DateTime? ExpectedDeliveryDate { get; set; }

    public DateTime? ActualDeliveryDate { get; set; }

    public virtual AspNetUser Buyer { get; set; } = null!;

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual AspNetUser Seller { get; set; } = null!;
}
