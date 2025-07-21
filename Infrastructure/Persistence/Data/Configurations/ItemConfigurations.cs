using Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Data.Configurations
{
    public class ItemConfigurations : IEntityTypeConfiguration<Restaurant>
    {
        public void Configure(EntityTypeBuilder<Restaurant> builder)
        {
            builder
               .HasMany(r => r.Items)
               .WithOne(i => i.Restaurant)
               .HasForeignKey(i => i.RestaurantId)
               .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
