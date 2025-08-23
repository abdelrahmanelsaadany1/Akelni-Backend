using Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Data.Configurations
{
    public class PaymentConfigurations : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            // ✅ Fix: Use OrderId as foreign key, not Id
            builder
                 .HasOne(p => p.Order)
                 .WithOne(o => o.Payment)
                 .HasForeignKey<Payment>(p => p.OrderId)  // ✅ Changed from p.Id to p.OrderId
                 .OnDelete(DeleteBehavior.Cascade);

            // ✅ Add index for better performance
            builder.HasIndex(p => p.OrderId).IsUnique();
        }
    }
}