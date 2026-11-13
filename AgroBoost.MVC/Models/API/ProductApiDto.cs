namespace AgroBoost.MVC.Models.API;

public class ProductApiDto
{
    public int Id { get; set; }
    public string SellerId { get; set; } = string.Empty;
    public string? SellerName { get; set; }
    public string? OrganizationName { get; set; }
    public string? OrganizationType { get; set; }
    public int CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public int MinOrderQty { get; set; }
    public int StockQty { get; set; }
    public string? Images { get; set; }
    public string? Certifications { get; set; }
    public int? ShelfLifeDays { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? District { get; set; }
    public string? State { get; set; }
}