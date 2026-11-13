using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AgroBoost.MVC.Models.Domain;

namespace AgroBoost.MVC.Data.EfCore.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.BuyerId)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(o => o.SellerId)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(o => o.OrderNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(o => o.Status)
            .HasMaxLength(20)
            .IsRequired()
            .HasDefaultValue("Pending");

        builder.Property(o => o.TotalAmount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(o => o.ShippingAddress)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(o => o.PaymentMethod)
            .HasMaxLength(20)
            .IsRequired()
            .HasDefaultValue("Cash on Delivery");

        builder.Property(o => o.PaymentStatus)
            .HasMaxLength(20)
            .IsRequired()
            .HasDefaultValue("Pending");

        builder.Property(o => o.OrderDate)
            .HasDefaultValueSql("GETUTCDATE()");

        // Relationships
        builder.HasOne(o => o.Buyer)
            .WithMany(u => u.OrdersAsBuyer)
            .HasForeignKey(o => o.BuyerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.Seller)
            .WithMany(u => u.OrdersAsSeller)
            .HasForeignKey(o => o.SellerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(o => o.BuyerId);
        builder.HasIndex(o => o.SellerId);
        builder.HasIndex(o => o.OrderNumber).IsUnique();
        builder.HasIndex(o => o.Status);
        builder.HasIndex(o => o.OrderDate);
    }
}