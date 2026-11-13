using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AgroBoost.MVC.Data.EfCore;
using AgroBoost.MVC.Models.Domain;
using AgroBoost.MVC.Models.API;
using AgroBoost.MVC.Models.DTOs;
using AgroBoost.MVC.Data.AdoNet.Repository.Interfaces;

namespace AgroBoost.MVC.Controllers.API;

[Route("api/products")]
[ApiController]
public class ProductsApiController : ControllerBase
{
    private readonly AgroBoostDbContext _context;
    private readonly IProductSearchRepository _searchRepo;

    public ProductsApiController(AgroBoostDbContext context, IProductSearchRepository searchRepo)
    {
        _context = context;
        _searchRepo = searchRepo;
    }

    // GET: api/products
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductApiDto>>> GetProducts(
        [FromQuery] string? keyword,
        [FromQuery] int? categoryId,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12)
    {
        var searchDto = new ProductSearchDto
        {
            Keyword = keyword,
            CategoryId = categoryId,
            MinPrice = minPrice,
            MaxPrice = maxPrice,
            PageNumber = page,
            PageSize = pageSize
        };

        var (items, total) = await _searchRepo.SearchAsync(searchDto);
        
        Response.Headers.Append("X-Total-Count", total.ToString());
        Response.Headers.Append("X-Page", page.ToString());
        Response.Headers.Append("X-Page-Size", pageSize.ToString());

        var dtos = items.Select(i => new ProductApiDto
{
    Id = i.Id,
    Name = i.Name,
    Description = i.Description,
    Unit = i.Unit,
    BasePrice = i.BasePrice,
    MinOrderQty = 1,
    StockQty = i.StockQty,
    CategoryName = i.CategoryName,
    SellerName = i.SellerName,
    OrganizationName = i.OrganizationName,
    OrganizationType = i.OrganizationType,
    District = i.District,
    State = i.State,
    Images = i.Images,
    Certifications = i.Certifications
}).ToList();
        return Ok(dtos);
    }

    // GET: api/products/5
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductApiDto>> GetProduct(int id)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Seller)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
            return NotFound();

        return Ok(MapToApiDto(product));
    }

    // POST: api/products
    [HttpPost]
    public async Task<ActionResult<ProductApiDto>> CreateProduct(ProductApiDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var product = new Product
        {
            SellerId = dto.SellerId,
            CategoryId = dto.CategoryId,
            Name = dto.Name,
            Description = dto.Description,
            Unit = dto.Unit,
            BasePrice = dto.BasePrice,
            MinOrderQty = dto.MinOrderQty,
            StockQty = dto.StockQty,
            Certifications = dto.Certifications,
            ShelfLifeDays = dto.ShelfLifeDays,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var createdDto = MapToApiDto(product);
        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, createdDto);
    }

    // PUT: api/products/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, ProductApiDto dto)
    {
        if (id != dto.Id)
            return BadRequest();

        var product = await _context.Products.FindAsync(id);
        if (product == null)
            return NotFound();

        product.Name = dto.Name;
        product.Description = dto.Description;
        product.CategoryId = dto.CategoryId;
        product.Unit = dto.Unit;
        product.BasePrice = dto.BasePrice;
        product.MinOrderQty = dto.MinOrderQty;
        product.StockQty = dto.StockQty;
        product.Certifications = dto.Certifications;
        product.ShelfLifeDays = dto.ShelfLifeDays;
        product.IsActive = dto.IsActive;
        product.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/products/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
            return NotFound();

        product.IsActive = false;
        product.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static ProductApiDto MapToApiDto(Product product)
    {
        return new ProductApiDto
        {
            Id = product.Id,
            SellerId = product.SellerId,
            SellerName = product.Seller?.FullName,
            OrganizationName = product.Seller?.OrganizationName,
            OrganizationType = product.Seller?.OrganizationType,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name,
            Name = product.Name,
            Description = product.Description,
            Unit = product.Unit,
            BasePrice = product.BasePrice,
            MinOrderQty = product.MinOrderQty,
            StockQty = product.StockQty,
            Images = product.Images,
            Certifications = product.Certifications,
            ShelfLifeDays = product.ShelfLifeDays,
            IsActive = product.IsActive,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }
}