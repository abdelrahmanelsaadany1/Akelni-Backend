using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Payment:BaseEntity
    {
        public int Id { get; set; }
        public string StripePaymentIntentId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaidAt { get; set; }
        public int OrderId { get; set; }
        public virtual Order Order { get; set; }

    }
}
