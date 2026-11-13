using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AgroBoost.MVC.Models.Domain;

namespace AgroBoost.MVC.Data.EfCore.Configurations;

public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("CartItems");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.BuyerId)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(c => c.Quantity)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(c => c.AddedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // Relationships
        builder.HasOne(c => c.Buyer)
            .WithMany(u => u.CartItems)
            .HasForeignKey(c => c.BuyerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.Product)
            .WithMany(p => p.CartItems)
            .HasForeignKey(c => c.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique constraint: one cart item per user per product
        builder.HasIndex(c => new { c.BuyerId, c.ProductId }).IsUnique();

        // Indexes
        builder.HasIndex(c => c.BuyerId);
    }
}