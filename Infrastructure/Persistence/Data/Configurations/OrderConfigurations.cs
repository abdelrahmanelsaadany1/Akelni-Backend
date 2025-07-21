using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class OrderConfigurations : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        // Configure Restaurant relationship
        builder
            .HasOne(o => o.Restaurant)
            .WithMany(r => r.Orders)
            .HasForeignKey(o => o.RestaurantId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure Order Status enum conversion
        builder
            .Property(o => o.Status)
            .HasConversion<string>();

        // Configure Payment relationship (one-to-one)
        builder
            .HasOne(o => o.Payment)
            .WithOne(p => p.Order)
            .HasForeignKey<Payment>(p => p.OrderId);
    }
}