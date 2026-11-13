using System.Data;
using System.Data.SqlClient;
using AgroBoost.MVC.Data.AdoNet.Repository.Interfaces;
using AgroBoost.MVC.Models.DTOs;
using Microsoft.Extensions.Configuration;

namespace AgroBoost.MVC.Data.AdoNet.Repository.Implementations;

public class SellerAnalyticsRepository : ISellerAnalyticsRepository
{
    private readonly string _connectionString;

    public SellerAnalyticsRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new ArgumentNullException(nameof(configuration), "DefaultConnection not found");
    }

    public async Task<SellerAnalyticsDto?> GetDashboardAsync(string sellerId, int monthsBack = 12)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand("sp_SellerDashboardAnalytics", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.AddWithValue("@SellerId", sellerId);
        command.Parameters.AddWithValue("@MonthsBack", monthsBack);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        SellerAnalyticsDto? analytics = null;

        // First result set: summary from view
        if (await reader.ReadAsync())
        {
            analytics = new SellerAnalyticsDto
            {
                SellerId = reader.GetString(reader.GetOrdinal("SellerId")),
                OrganizationName = reader.GetString(reader.GetOrdinal("OrganizationName")),
                TotalProducts = reader.GetInt32(reader.GetOrdinal("TotalProducts")),
                ActiveProducts = reader.GetInt32(reader.GetOrdinal("ActiveProducts")),
                TotalInventoryUnits = reader.GetInt32(reader.GetOrdinal("TotalInventoryUnits")),
                TotalOrders = reader.GetInt32(reader.GetOrdinal("TotalOrders")),
                CompletedOrders = reader.GetInt32(reader.GetOrdinal("CompletedOrders")),
                TotalRevenue = reader.GetDecimal(reader.GetOrdinal("TotalRevenue")),
                AvgOrderValue = reader.IsDBNull(reader.GetOrdinal("AvgOrderValue")) ? 0 : reader.GetDecimal(reader.GetOrdinal("AvgOrderValue")),
                LastOrderDate = reader.IsDBNull(reader.GetOrdinal("LastOrderDate")) ? null : reader.GetDateTime(reader.GetOrdinal("LastOrderDate"))
            };
        }

        if (analytics == null)
            return null;

        // Second result set: monthly revenue trend
        if (await reader.NextResultAsync())
        {
            while (await reader.ReadAsync())
            {
                analytics.MonthlyRevenue.Add(new MonthlyRevenueDto
                {
                    Month = reader.GetString(reader.GetOrdinal("Month")),
                    Revenue = reader.GetDecimal(reader.GetOrdinal("Revenue")),
                    Orders = reader.GetInt32(reader.GetOrdinal("Orders")),
                    CumulativeRevenue = reader.GetDecimal(reader.GetOrdinal("CumulativeRevenue"))
                });
            }
        }

        // Third result set: top products
        if (await reader.NextResultAsync())
        {
            while (await reader.ReadAsync())
            {
                analytics.TopProducts.Add(new TopProductDto
                {
                    ProductName = reader.GetString(reader.GetOrdinal("Name")),
                    ProductRevenue = reader.GetDecimal(reader.GetOrdinal("ProductRevenue")),
                    UnitsSold = reader.GetInt32(reader.GetOrdinal("UnitsSold"))
                });
            }
        }

        // Fourth result set: order funnel
        if (await reader.NextResultAsync())
        {
            while (await reader.ReadAsync())
            {
                analytics.OrderFunnel.Add(new OrderFunnelDto
                {
                    Status = reader.GetString(reader.GetOrdinal("Status")),
                    Count = reader.GetInt32(reader.GetOrdinal("Count"))
                });
            }
        }

        return analytics;
    }
}