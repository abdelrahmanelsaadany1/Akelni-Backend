using Domain.Dtos.OrderReportsDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Abstractions.IServices
{
    public interface IOrderReportService
    {
        Task<List<OrderDto>> GetOrdersAsync(string chefId, DateTime from, DateTime to);
        Task<List<OrderItemDto>> GetOrderItemsAsync(List<int> orderIds);
        Task<OrderSummaryDto> GetOrderSummaryAsync(string chefId, DateTime from, DateTime to);
        Task<byte[]> ExportOrdersToExcelAsync(string chefId, ExportOrdersDetailsDto dto);
    }
}
