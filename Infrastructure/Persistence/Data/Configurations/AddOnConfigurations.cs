using Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Data.Configurations
{
    public class AddOnConfigurations : IEntityTypeConfiguration<ItemAddOn>
    {
        public void Configure(EntityTypeBuilder<ItemAddOn> builder)
        {
            builder
                           .HasKey(ia => new { ia.ItemId, ia.AddOnId });
            builder
                .HasOne(ia => ia.Item)
                .WithMany(i => i.ItemAddOns)
                .HasForeignKey(ia => ia.ItemId);
            builder
                .HasOne(ia => ia.AddOn)
                .WithMany(a => a.ItemAddOns)
                .HasForeignKey(ia => ia.AddOnId);

        }
    }
}
