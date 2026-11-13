using System.Data;
using System.Data.SqlClient;
using AgroBoost.MVC.Data.AdoNet.Repository.Interfaces;
using AgroBoost.MVC.Models.DTOs;
using Microsoft.Extensions.Configuration;

namespace AgroBoost.MVC.Data.AdoNet.Repository.Implementations;

public class AdminReportRepository : IAdminReportRepository
{
    private readonly string _connectionString;

    public AdminReportRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new ArgumentNullException(nameof(configuration), "DefaultConnection not found");
    }

    public async Task<AdminReportDto> GetPlatformReportAsync()
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand("sp_AdminPlatformReport", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        var report = new AdminReportDto();

        // First result set: summary stats
        if (await reader.ReadAsync())
        {
            report.TotalUsers = reader.GetInt32(reader.GetOrdinal("TotalUsers"));
            report.TotalSellers = reader.GetInt32(reader.GetOrdinal("TotalSellers"));
            report.TotalBuyers = reader.GetInt32(reader.GetOrdinal("TotalBuyers"));
            report.VerifiedSellers = reader.GetInt32(reader.GetOrdinal("VerifiedSellers"));
            report.PendingVerification = reader.GetInt32(reader.GetOrdinal("PendingVerification"));
            report.TotalProducts = reader.GetInt32(reader.GetOrdinal("TotalProducts"));
            report.ActiveProducts = reader.GetInt32(reader.GetOrdinal("ActiveProducts"));
            report.TotalOrders = reader.GetInt32(reader.GetOrdinal("TotalOrders"));
            report.TotalRevenue = reader.GetDecimal(reader.GetOrdinal("TotalRevenue"));
        }

        // Second result set: category performance
        if (await reader.NextResultAsync())
        {
            while (await reader.ReadAsync())
            {
                report.CategoryPerformance.Add(new CategoryPerformanceDto
                {
                    CategoryName = reader.GetString(reader.GetOrdinal("CategoryName")),
                    ProductCount = reader.GetInt32(reader.GetOrdinal("ProductCount")),
                    OrderCount = reader.GetInt32(reader.GetOrdinal("OrderCount")),
                    Revenue = reader.GetDecimal(reader.GetOrdinal("Revenue"))
                });
            }
        }

        // Third result set: geographic distribution
        if (await reader.NextResultAsync())
        {
            while (await reader.ReadAsync())
            {
                report.GeographicDistribution.Add(new GeographicDistributionDto
                {
                    State = reader.GetString(reader.GetOrdinal("State")),
                    SellerCount = reader.GetInt32(reader.GetOrdinal("SellerCount")),
                    ProductCount = reader.GetInt32(reader.GetOrdinal("ProductCount"))
                });
            }
        }

        return report;
    }
}