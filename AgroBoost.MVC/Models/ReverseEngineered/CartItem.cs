using System;
using System.Collections.Generic;

namespace AgroBoost.MVC.Models.ReverseEngineered;

public partial class CartItem
{
    public int Id { get; set; }

    public string BuyerId { get; set; } = null!;

    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public DateTime AddedAt { get; set; }

    public virtual AspNetUser Buyer { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
