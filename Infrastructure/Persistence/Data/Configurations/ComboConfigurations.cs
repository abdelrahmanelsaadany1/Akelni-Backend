using Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Data.Configurations
{
    public class ComboConfigurations : IEntityTypeConfiguration<ItemCombo>
    {
        public void Configure(EntityTypeBuilder<ItemCombo> builder)
        {
            builder
                .HasKey(ic => new { ic.ItemId, ic.ComboId });
            builder
                .HasOne(ic => ic.Item)
                .WithMany(i => i.ItemCombos)
                .HasForeignKey(ic => ic.ItemId);
            builder
                .HasOne(ic => ic.Combo)
                .WithMany(c => c.ItemCombos)
                .HasForeignKey(ic => ic.ComboId);
        }
    }
}
