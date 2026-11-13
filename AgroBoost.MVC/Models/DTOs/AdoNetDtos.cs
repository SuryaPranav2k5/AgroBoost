namespace AgroBoost.MVC.Models.DTOs;

public class ProductSearchDto
{
    public string? Keyword { get; set; }
    public int? CategoryId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? SellerType { get; set; } // SHG, FPG
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 12;
    public string? SortBy { get; set; } = "CreatedAt";
    public string? SortDir { get; set; } = "DESC";
}

public class ProductSearchResultDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public int StockQty { get; set; }
    public string? Images { get; set; }
    public string? Certifications { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string SellerName { get; set; } = string.Empty;
    public string? OrganizationName { get; set; }
    public string? OrganizationType { get; set; }
    public string? District { get; set; }
    public string? State { get; set; }
}

public class SellerAnalyticsDto
{
    public string SellerId { get; set; } = string.Empty;
    public string OrganizationName { get; set; } = string.Empty;
    public int TotalProducts { get; set; }
    public int ActiveProducts { get; set; }
    public int TotalInventoryUnits { get; set; }
    public int TotalOrders { get; set; }
    public int CompletedOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AvgOrderValue { get; set; }
    public DateTime? LastOrderDate { get; set; }
    public List<MonthlyRevenueDto> MonthlyRevenue { get; set; } = new();
    public List<TopProductDto> TopProducts { get; set; } = new();
    public List<OrderFunnelDto> OrderFunnel { get; set; } = new();
}

public class MonthlyRevenueDto
{
    public string Month { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int Orders { get; set; }
    public decimal CumulativeRevenue { get; set; }
}

public class TopProductDto
{
    public string ProductName { get; set; } = string.Empty;
    public decimal ProductRevenue { get; set; }
    public int UnitsSold { get; set; }
}

public class OrderFunnelDto
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class AdminReportDto
{
    public int TotalUsers { get; set; }
    public int TotalSellers { get; set; }
    public int TotalBuyers { get; set; }
    public int VerifiedSellers { get; set; }
    public int PendingVerification { get; set; }
    public int TotalProducts { get; set; }
    public int ActiveProducts { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public List<CategoryPerformanceDto> CategoryPerformance { get; set; } = new();
    public List<GeographicDistributionDto> GeographicDistribution { get; set; } = new();
}

public class CategoryPerformanceDto
{
    public string CategoryName { get; set; } = string.Empty;
    public int ProductCount { get; set; }
    public int OrderCount { get; set; }
    public decimal Revenue { get; set; }
}

public class GeographicDistributionDto
{
    public string State { get; set; } = string.Empty;
    public int SellerCount { get; set; }
    public int ProductCount { get; set; }
}