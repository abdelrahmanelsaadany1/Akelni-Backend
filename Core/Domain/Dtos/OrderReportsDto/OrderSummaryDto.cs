using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dtos.OrderReportsDto
{
    public  class OrderSummaryDto
    {
        public int TotalOrders { get; set; }
        public decimal TotalSales { get; set; }
        public decimal Revenue { get; set; }
        public decimal AverageOrderValue { get; set; }
        public int PeakOrderHour { get; set; }
        public string TopSellingItem { get; set; } = string.Empty;
        public int TopSellingItemQuantity { get; set; }
    }
}
