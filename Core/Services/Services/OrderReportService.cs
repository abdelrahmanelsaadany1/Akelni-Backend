using ClosedXML.Excel;
using Domain.Dtos.OrderReportsDto;
using Microsoft.EntityFrameworkCore;
using Persistence.Data;
using Services.Abstractions.IServices;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Services
{
    public class OrderReportService : IOrderReportService
    {
        private readonly FoodCourtDbContext _dbContext;
        private readonly IdentityContext _identityContext;

        public OrderReportService(FoodCourtDbContext dbContext, IdentityContext identityContext)
        {
            _dbContext = dbContext;
            _identityContext = identityContext;
        }

        public async Task<List<OrderDto>> GetOrdersAsync(string chefId, DateTime from, DateTime to)
        {
            var restaurantId = await GetRestaurantIdAsync(chefId);

            var ordersList = await _dbContext.Orders
                .Where(o => o.RestaurantId == restaurantId && o.CreatedAt >= from && o.CreatedAt < to)
                .Select(order => new
                {
                    order.Id,
                    order.CustomerId,
                    order.CreatedAt,
                    order.DeliveryFee,
                    order.PlatformFee,
                    order.DistanceKm,
                    order.SubTotal
                })
                .ToListAsync();

            var customerNames = await GetCustomerNamesAsync(ordersList.Select(o => o.CustomerId).Distinct());

            return ordersList.Select(o => new OrderDto
            {
                OrderId = o.Id,
                CustomerId = o.CustomerId,
                CustomerName = customerNames.GetValueOrDefault(o.CustomerId).DisplayName ?? "",
                CustomerEmail = customerNames.GetValueOrDefault(o.CustomerId).Email ?? "",
                CustomerPhone = customerNames.GetValueOrDefault(o.CustomerId).PhoneNumber ?? "",
                CreatedAt = o.CreatedAt,
                Date = o.CreatedAt.ToString("MM/dd/yyyy"),
                Time = o.CreatedAt.ToString("HH:mm"),
                DeliveryFee = o.DeliveryFee,
                PlatformFee = o.PlatformFee,
                DistanceKm = o.DistanceKm,
                Total = o.SubTotal
            }).ToList();
        }

        public async Task<List<OrderItemDto>> GetOrderItemsAsync(List<int> orderIds)
        {
            return await _dbContext.OrderItems
                .Where(oi => orderIds.Contains(oi.OrderId))
                .Select(oi => new OrderItemDto
                {
                    OrderId = oi.OrderId,
                    ItemId = oi.ItemId,
                    ItemName = oi.Item.Name,
                    Quantity = oi.Quantity,
                    ItemPrice = oi.ItemPrice,
                    TotalPrice = oi.TotalPrice
                })
                .ToListAsync();
        }

        public async Task<OrderSummaryDto> GetOrderSummaryAsync(string chefId, DateTime from, DateTime to)
        {
            var restaurantId = await GetRestaurantIdAsync(chefId);

            var orderStats = await _dbContext.Orders
                .Where(o => o.RestaurantId == restaurantId && o.CreatedAt >= from && o.CreatedAt < to)
                .GroupBy(x => true)
                .Select(x => new
                {
                    Count = x.Count(),
                    TotalSales = x.Sum(y => y.SubTotal),
                    PlatformFee = x.Sum(y => y.PlatformFee),
                    DeliveryFee = x.Sum(y => y.DeliveryFee)
                })
                .FirstOrDefaultAsync();

            if (orderStats == null)
                return new OrderSummaryDto();

            var revenue = orderStats.TotalSales - orderStats.PlatformFee - orderStats.DeliveryFee;
            var avgOrderValue = orderStats.Count > 0 ? revenue / orderStats.Count : 0;

            var peakHour = await GetPeakOrderHourAsync(restaurantId , from ,to);
            var topSellingItem = await GetTopSellingItemAsync(restaurantId, from, to);

            return new OrderSummaryDto
            {
                TotalOrders = orderStats.Count,
                TotalSales = orderStats.TotalSales,
                Revenue = revenue,
                AverageOrderValue = avgOrderValue,
                PeakOrderHour = peakHour,
                TopSellingItem = topSellingItem.ItemName,
                TopSellingItemQuantity = topSellingItem.Quantity
            };
        }

        public async Task<byte[]> ExportOrdersToExcelAsync(string chefId, ExportOrdersDetailsDto dto)
        {
          
            var orders = await GetOrdersAsync(chefId, dto.from, dto.to);
            var orderItems = await GetOrderItemsAsync(orders.Select(o => o.OrderId).ToList());
            var orderSummery = await GetOrderSummaryAsync(chefId, dto.from, dto.to);

            using var workbook = new XLWorkbook();

            // Add Orders sheet
            var ordersTable = CreateOrdersDataTable(orders);
            workbook.AddWorksheet(ordersTable, "Orders Details");

            // Add Order Items sheet
            var itemsTable = CreateOrderItemsDataTable(orderItems);
            workbook.AddWorksheet(itemsTable, "Order Items");

            // Add Order Summery sheet
            var summeryTable = CreateOrderSummaryDataTable(orderSummery);
            workbook.AddWorksheet(summeryTable, "Orders Summery");

            using var stream = new MemoryStream();
            stream.Position = 0;
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        private async Task<int> GetRestaurantIdAsync(string chefId)
        {
            return await _dbContext.Restaurants
                .Where(r => r.ChefId == chefId)
                .Select(r => r.Id)
                .FirstOrDefaultAsync();
        }

        private async Task<Dictionary<string, (string DisplayName, string Email, string PhoneNumber)>> GetCustomerNamesAsync(IEnumerable<string> customerIds)
        {
            return await _identityContext.Users
                .Where(u => customerIds.Contains(u.Id))
                .Select(u => new { u.Id, u.DisplayName, u.Email, u.PhoneNumber })
                .ToDictionaryAsync(u => u.Id, u => (u.DisplayName, u.Email, u.PhoneNumber));
        }

        private async Task<int> GetPeakOrderHourAsync(int restaurantId, DateTime? from, DateTime? to)
        {
            return await _dbContext.Orders
                .Where(o => o.RestaurantId == restaurantId && o.CreatedAt >= from && o.CreatedAt < to)
                .GroupBy(o => o.CreatedAt.Hour)
                .OrderBy(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefaultAsync();
        }

        private async Task<(string ItemName, int Quantity)> GetTopSellingItemAsync(int restaurantId, DateTime from, DateTime to)
        {
            var result = await _dbContext.OrderItems
                .Where(oi => oi.Order.RestaurantId == restaurantId &&
                           oi.Order.CreatedAt >= from &&
                           oi.Order.CreatedAt < to)
                .GroupBy(oi => oi.Item.Name)
                .OrderByDescending(g => g.Sum(x => x.Quantity))
                .Select(g => new { ItemName = g.Key, Quantity = g.Sum(x => x.Quantity) })
                .FirstOrDefaultAsync();

            return (result?.ItemName ?? "", result?.Quantity ?? 0);
        }

        private static DataTable CreateOrdersDataTable(List<OrderDto> orders)
        {
            var dataTable = new DataTable();
            dataTable.Columns.AddRange(new[]
            {
                new DataColumn("Order Id", typeof(int)),
                new DataColumn("Customer Id", typeof(string)),
                new DataColumn("Customer Name", typeof(string)),
                new DataColumn("Customer Email", typeof(string)),
                new DataColumn("Customer Phone", typeof(string)),
                new DataColumn("Date", typeof(string)),
                new DataColumn("Time", typeof(string)),
                new DataColumn("Delivery Fee", typeof(decimal)),
                new DataColumn("Platform Fee", typeof(decimal)),
                new DataColumn("Distance Km", typeof(double)),
                new DataColumn("Total", typeof(decimal))
            });

            foreach (var order in orders)
            {
                dataTable.Rows.Add(
                    order.OrderId, order.CustomerId, order.CustomerName,
                    order.CustomerEmail, order.CustomerPhone, order.Date,
                    order.Time, order.DeliveryFee, order.PlatformFee,
                    order.DistanceKm, order.Total
                );
            }

            return dataTable;
        }

        private static DataTable CreateOrderSummaryDataTable(OrderSummaryDto order)
        {
            var dataTable = new DataTable();
            dataTable.Columns.AddRange(new[]
            {
                new DataColumn("total Orders", typeof(int)),
                new DataColumn("totalSales", typeof(float)),
                new DataColumn("revenue", typeof(float)),
                new DataColumn("average Order Value", typeof(float)),
                new DataColumn("peak Order Hour", typeof(string)),
                new DataColumn("top Selling Item", typeof(string)),
                new DataColumn("top Selling Item Quantity", typeof(int))
            });

         
                dataTable.Rows.Add(
                    order.TotalOrders, 
                    order.TotalSales, 
                    order.Revenue,
                    order.AverageOrderValue,
                    order.PeakOrderHour,
                    order.TopSellingItem,
                    order.TopSellingItemQuantity
                );
            

            return dataTable;
        }
        private static DataTable CreateOrderItemsDataTable(List<OrderItemDto> orderItems)
        {
            var dataTable = new DataTable();
            dataTable.Columns.AddRange(new[]
            {
                new DataColumn("Order Id", typeof(int)),
                new DataColumn("Item Id", typeof(int)),
                new DataColumn("Item Name", typeof(string)),
                new DataColumn("Quantity", typeof(int)),
                new DataColumn("Item Price", typeof(decimal)),
                new DataColumn("Total Price", typeof(decimal))
            });

            foreach (var item in orderItems)
            {
                dataTable.Rows.Add(
                    item.OrderId, item.ItemId, item.ItemName,
                    item.Quantity, item.ItemPrice, item.TotalPrice
                );
            }

            return dataTable;
        }
      }
    }
