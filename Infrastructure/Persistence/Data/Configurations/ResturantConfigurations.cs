using Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class RestaurantConfigurations : IEntityTypeConfiguration<Restaurant>
{
    public void Configure(EntityTypeBuilder<Restaurant> builder)
    {
        builder.Property(r => r.ChefId)
               .IsRequired();

        builder.Property(r => r.Name)
               .HasMaxLength(100)
               .IsRequired();

        // Optional: you can configure indexes or other fields if needed
    }
}
