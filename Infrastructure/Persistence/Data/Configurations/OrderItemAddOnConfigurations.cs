using Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Data.Configurations
{
    public class OrderItemAddOnConfigurations : IEntityTypeConfiguration<OrderItemAddOn>
    {
        public void Configure(EntityTypeBuilder<OrderItemAddOn> builder)
        {
            builder
                .HasKey(oia => new { oia.OrderItemId, oia.AddOnId });
            builder
                .HasOne(oia => oia.OrderItem)
                .WithMany(oi => oi.AddOns)
                .HasForeignKey(oia => oia.OrderItemId);
            builder
                .HasOne(oia => oia.AddOn)
                .WithMany(a => a.OrderItemAddOns)
                .HasForeignKey(oia => oia.AddOnId);

        }
    }
}
