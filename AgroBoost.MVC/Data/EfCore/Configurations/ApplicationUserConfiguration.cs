using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AgroBoost.MVC.Models.Domain;

namespace AgroBoost.MVC.Data.EfCore.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable("AspNetUsers");

        builder.Property(u => u.FullName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.UserType)
            .HasMaxLength(20)
            .HasDefaultValue("Buyer")
            .IsRequired();

        builder.Property(u => u.OrganizationName)
            .HasMaxLength(200);

        builder.Property(u => u.OrganizationType)
            .HasMaxLength(20);

        builder.Property(u => u.Address)
            .HasMaxLength(500);

        builder.Property(u => u.District)
            .HasMaxLength(100);

        builder.Property(u => u.State)
            .HasMaxLength(100);

        builder.Property(u => u.GSTNumber)
            .HasMaxLength(20);

        builder.Property(u => u.RegistrationNumber)
            .HasMaxLength(50);

        builder.Property(u => u.IsVerified)
            .HasDefaultValue(false);

        builder.Property(u => u.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(u => u.IsActive)
            .HasDefaultValue(true);

        // Indexes
        builder.HasIndex(u => u.UserType);
        builder.HasIndex(u => u.OrganizationType);
    }
}