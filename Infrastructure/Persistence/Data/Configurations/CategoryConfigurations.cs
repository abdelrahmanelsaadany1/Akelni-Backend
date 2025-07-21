using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Data.Configurations
{
    public class CategoryConfigurations : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.HasMany(c => c.Items)
                   .WithOne(i => i.Category)
                   .HasForeignKey(i => i.CategoryId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
