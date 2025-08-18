using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dtos.OrderReportsDto
{
    public class ExportOrdersDetailsDto
    {
        public DateTime from { get; set; } = new DateTime(2025, 1, 1);
        public DateTime to { get; set; } = DateTime.Now;

        
    }
}
