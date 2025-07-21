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
            builder
                 .HasOne(p => p.Order)
                 .WithOne(o => o.Payment)
                 .HasForeignKey<Payment>(p => p.Id)
                 .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
