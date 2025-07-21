using Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Data.Configurations
{
    public class OrderItemComboConfigurations : IEntityTypeConfiguration<OrderItemCombo>
    {
        public void Configure(EntityTypeBuilder<OrderItemCombo> builder)
        {
            builder
                .HasKey(oic => new { oic.OrderItemId, oic.ComboId });
            builder
                .HasOne(oic => oic.OrderItem)
                .WithMany(oi => oi.Combos)
                .HasForeignKey(oic => oic.OrderItemId);
            builder
                .HasOne(oic => oic.Combo)
                .WithMany(c => c.OrderItemCombos)
                .HasForeignKey(oic => oic.ComboId);

        }
    }
}
