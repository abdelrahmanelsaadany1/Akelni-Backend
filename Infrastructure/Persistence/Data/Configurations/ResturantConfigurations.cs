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

        builder.HasIndex(r => r.Name);
        builder.HasIndex(r => r.Rating);
        builder.HasIndex(r => r.IsOpen);
        builder.HasIndex(r => new { r.IsOpen, r.Rating });
    }
}
