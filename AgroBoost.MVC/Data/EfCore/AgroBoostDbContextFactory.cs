using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using AgroBoost.MVC.Data.EfCore;

namespace AgroBoost.MVC.Data.EfCore;

public class AgroBoostDbContextFactory : IDesignTimeDbContextFactory<AgroBoostDbContext>
{
    public AgroBoostDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<AgroBoostDbContext>();
        optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));

        return new AgroBoostDbContext(optionsBuilder.Options);
    }
}