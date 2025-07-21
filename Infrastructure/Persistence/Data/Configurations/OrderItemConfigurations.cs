using Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Data.Configurations
{
    public class OrderItemConfigurations : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder
              .HasOne(oi => oi.Order)
              .WithMany(o => o.Items)
              .HasForeignKey(oi => oi.OrderId)
              .OnDelete(DeleteBehavior.Cascade);
            builder
                .HasOne(oi => oi.Item)
                .WithMany()
                .HasForeignKey(oi => oi.ItemId)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
