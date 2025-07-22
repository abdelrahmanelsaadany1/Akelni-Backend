using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dtos.OrderDto
{
    public class PaymentResponseDto
    {
        public int Id { get; set; } // From BaseEntity
        public string StripePaymentIntentId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaidAt { get; set; }
        public int OrderId { get; set; }
    }
}
