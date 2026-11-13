using System.Data;
using System.Data.SqlClient;
using AgroBoost.MVC.Data.AdoNet.Repository.Interfaces;
using AgroBoost.MVC.Models.DTOs;
using Microsoft.Extensions.Configuration;

namespace AgroBoost.MVC.Data.AdoNet.Repository.Implementations;

public class ProductSearchRepository : IProductSearchRepository
{
    private readonly string _connectionString;

    public ProductSearchRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new ArgumentNullException(nameof(configuration), "DefaultConnection not found");
    }

    public async Task<(IEnumerable<ProductSearchResultDto> Items, int Total)> SearchAsync(ProductSearchDto dto)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand("sp_AdvancedProductSearch", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.AddWithValue("@Keyword", dto.Keyword ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@CategoryId", dto.CategoryId ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@MinPrice", dto.MinPrice ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@MaxPrice", dto.MaxPrice ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@SellerType", dto.SellerType ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@PageNumber", dto.PageNumber);
        command.Parameters.AddWithValue("@PageSize", dto.PageSize);
        command.Parameters.AddWithValue("@SortBy", dto.SortBy ?? "CreatedAt");
        command.Parameters.AddWithValue("@SortDir", dto.SortDir ?? "DESC");

        var items = new List<ProductSearchResultDto>();
        
        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();
        
        // First result set: products
        while (await reader.ReadAsync())
        {
            items.Add(MapProductResult(reader));
        }

        // Second result set: total count
        int total = 0;
        if (await reader.NextResultAsync() && await reader.ReadAsync())
        {
            total = reader.GetInt32(0);
        }

        return (items, total);
    }

    private static ProductSearchResultDto MapProductResult(SqlDataReader reader)
    {
        return new ProductSearchResultDto
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            Name = reader.GetString(reader.GetOrdinal("Name")),
            Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
            Unit = reader.GetString(reader.GetOrdinal("Unit")),
            BasePrice = reader.GetDecimal(reader.GetOrdinal("BasePrice")),
            StockQty = reader.GetInt32(reader.GetOrdinal("StockQty")),
            Images = reader.IsDBNull(reader.GetOrdinal("Images")) ? null : reader.GetString(reader.GetOrdinal("Images")),
            Certifications = reader.IsDBNull(reader.GetOrdinal("Certifications")) ? null : reader.GetString(reader.GetOrdinal("Certifications")),
            CategoryName = reader.GetString(reader.GetOrdinal("CategoryName")),
            SellerName = reader.GetString(reader.GetOrdinal("SellerName")),
            OrganizationName = reader.IsDBNull(reader.GetOrdinal("OrganizationName")) ? null : reader.GetString(reader.GetOrdinal("OrganizationName")),
            OrganizationType = reader.IsDBNull(reader.GetOrdinal("OrganizationType")) ? null : reader.GetString(reader.GetOrdinal("OrganizationType")),
            District = reader.IsDBNull(reader.GetOrdinal("District")) ? null : reader.GetString(reader.GetOrdinal("District")),
            State = reader.IsDBNull(reader.GetOrdinal("State")) ? null : reader.GetString(reader.GetOrdinal("State"))
        };
    }
}