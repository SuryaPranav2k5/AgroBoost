using AgroBoost.MVC.Models.DTOs;

namespace AgroBoost.MVC.Data.AdoNet.Repository.Interfaces;

public interface IProductSearchRepository
{
    Task<(IEnumerable<ProductSearchResultDto> Items, int Total)> SearchAsync(ProductSearchDto dto);
}

public interface ISellerAnalyticsRepository
{
    Task<SellerAnalyticsDto?> GetDashboardAsync(string sellerId, int monthsBack = 12);
}

public interface IAdminReportRepository
{
    Task<AdminReportDto> GetPlatformReportAsync();
}