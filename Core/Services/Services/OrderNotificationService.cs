using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Dtos.OrderDto;
using Domain.Entities;
using FoodCourt.Hubs;
using Microsoft.AspNetCore.SignalR;
using Persistence.Data;
using Services.Abstractions.IServices;

namespace Services.Services
{
    public class OrderNotificationService : IOrderNotificationService
    {
        private readonly IHubContext<OrderHub> _hubContext;
        private readonly IdentityContext _dbContext;

        public OrderNotificationService(IHubContext<OrderHub> hubContext, IdentityContext dbContext)
        {
            _hubContext = hubContext;
            _dbContext = dbContext;
        }

        public async Task SendOrderRequestToChef(string chefId, OrderResponseDto order)
        {
            var totalAmount = CalculateOrderTotalWithAddOnsAndCombos(order);
            var customerName = order.CustomerName ?? "Customer";

            // Precompute summary in one pass
            int itemCount = order.Items?.Count ?? 0;
            int totalQuantity = 0, addOnCount = 0, comboCount = 0;
            bool hasAddOns = false, hasCombos = false;

            var items = new List<object>();

            if (order.Items != null)
            {
                foreach (var item in order.Items)
                {
                    totalQuantity += item.Quantity;
                    if (item.AddOns?.Count > 0)
                    {
                        addOnCount += item.AddOns.Count;
                        hasAddOns = true;
                    }
                    if (item.Combos?.Count > 0)
                    {
                        comboCount += item.Combos.Count;
                        hasCombos = true;
                    }

                    items.Add(new
                    {
                        id = item.Id,
                        itemId = item.ItemId,
                        itemName = item.ItemName,
                        itemPrice = item.ItemPrice,
                        quantity = item.Quantity,
                        totalPrice = item.TotalPrice,
                        addOns = item.AddOns?.Select(addOn => new
                        {
                            id = addOn.AddOnId,
                            name = addOn.AddOnName,
                            price = addOn.AddOnPrice
                        }).ToList(),
                        combos = item.Combos?.Select(combo => new
                        {
                            id = combo.ComboId,
                            name = combo.ComboName,
                            price = combo.ComboPrice
                        }).ToList(),
                        itemTotalWithExtras = CalculateItemTotalWithExtras(item)
                    });
                }
            }

            var payload = new
            {
                orderId = order.Id,
                customerName,
                items,
                totalAmount,
                subTotal = order.SubTotal,
                deliveryFee = order.DeliveryFee,
                platformFee = order.PlatformFee,
                createdAt = order.CreatedAt,
                restaurantName = order.RestaurantName,
                status = order.Status.ToString().ToLowerInvariant(),
                orderSummary = new
                {
                    itemCount,
                    totalQuantity,
                    hasAddOns,
                    hasCombos,
                    addOnCount,
                    comboCount
                }
            };

            await _hubContext.Clients.Group($"Chef_{chefId}")
                .SendCoreAsync("ReceiveOrderRequest", new object[] { payload });
        }


        public async Task NotifyCustomerOrderAccepted(string customerId, int orderId)
        {
            await _hubContext.Clients.Group($"Customer_{customerId}").SendAsync("OrderAccepted", new
            {
                orderId,
                message = "Your order has been accepted! Redirecting to payment...",
                timestamp = DateTime.UtcNow
            });
        }

        public async Task NotifyCustomerOrderRejected(string customerId, int orderId, string reason)
        {
            await _hubContext.Clients.Group($"Customer_{customerId}").SendAsync("OrderRejected", new
            {
                orderId,
                message = $"Your order has been rejected. Reason: {reason}",
                timestamp = DateTime.UtcNow
            });
        }

        public async Task NotifyChefOrderPaid(string chefId, int orderId)
        {
            await _hubContext.Clients.Group($"Chef_{chefId}").SendAsync("OrderPaid", new
            {
                orderId,
                message = "Customer has completed payment. You can start preparing the order.",
                timestamp = DateTime.UtcNow
            });
        }

        public async Task NotifyChefOrderCancelled(string chefId, int orderId)
        {
            await _hubContext.Clients.Group($"Chef_{chefId}").SendAsync("OrderPaymentCancelled", new
            {
                orderId,
                message = "Customer cancelled the payment. Do not prepare this order.",
                timestamp = DateTime.UtcNow
            });
        }

        public async Task NotifyCustomerOrderStatusUpdate(string customerId, int orderId, Order.OrderStatus status)
        {
            await _hubContext.Clients.Group($"Customer_{customerId}").SendAsync("OrderStatusUpdate", new
            {
                orderId,
                status = status.ToString().ToLowerInvariant(),
                message = GetStatusMessage(status),
                timestamp = DateTime.UtcNow
            });
        }

        public async Task NotifyChefNewOrder(string chefId, int orderId, object payload)
        {
            await _hubContext.Clients.Group($"Chef_{chefId}")
                .SendAsync("ReceiveOrderRequest", payload);
        }

        public async Task NotifyOrderStatusToChef(string chefId, int orderId, string status)
        {
            var normalized = (status ?? "pending").ToLowerInvariant();
            await _hubContext.Clients.Group($"Chef_{chefId}").SendAsync("OrderStatusUpdate", new
            {
                orderId,
                status = normalized,
                message = GetStatusMessage(ParseStatusSafe(normalized)),
                timestamp = DateTime.UtcNow
            });
        }

        public async Task NotifyOrderStatusToCustomer(string customerId, int orderId, string status)
        {
            var normalized = (status ?? "pending").ToLowerInvariant();
            await _hubContext.Clients.Group($"Customer_{customerId}").SendAsync("OrderStatusUpdate", new
            {
                orderId,
                status = normalized,
                message = GetStatusMessage(ParseStatusSafe(normalized)),
                timestamp = DateTime.UtcNow
            });
        }

        public async Task NotifyOrderPaid(string customerId, string chefId, int orderId, decimal amount)
        {
            var payload = new
            {
                orderId,
                amount,
                message = "Payment received.",
                timestamp = DateTime.UtcNow
            };
            await Task.WhenAll(
                _hubContext.Clients.Group($"Customer_{customerId}").SendAsync("OrderPaid", payload),
                _hubContext.Clients.Group($"Chef_{chefId}").SendAsync("OrderPaid", payload)
            );
        }

        public async Task NotifyOrderPaymentCancelled(string customerId, string chefId, int orderId, string reason)
        {
            var payload = new
            {
                orderId,
                reason,
                message = "Payment cancelled.",
                timestamp = DateTime.UtcNow
            };
            await Task.WhenAll(
                _hubContext.Clients.Group($"Customer_{customerId}").SendAsync("OrderPaymentCancelled", payload),
                _hubContext.Clients.Group($"Chef_{chefId}").SendAsync("OrderPaymentCancelled", payload)
            );
        }

        #region Private Helper Methods

        /// <summary>
        /// ✅ Calculates the total order amount including all add-ons and combos
        /// </summary>
        private decimal CalculateOrderTotalWithAddOnsAndCombos(OrderResponseDto order)
        {
            decimal total = 0;

            if (order.Items != null)
            {
                foreach (var item in order.Items)
                {
                    // Base item total
                    total += item.ItemPrice * item.Quantity;

                    // Add-ons total
                    if (item.AddOns != null)
                    {
                        total += item.AddOns.Sum(addOn => addOn.AddOnPrice * item.Quantity);
                    }

                    // Combos total
                    if (item.Combos != null)
                    {
                        total += item.Combos.Sum(combo => combo.ComboPrice * item.Quantity);
                    }
                }
            }

            // Add fees
            total += order.DeliveryFee + order.PlatformFee;

            return total;
        }

        /// <summary>
        /// ✅ Calculates individual item total including its add-ons and combos
        /// </summary>
        private decimal CalculateItemTotalWithExtras(OrderItemResponseDto item)
        {
            decimal itemTotal = item.ItemPrice * item.Quantity;

            // Add add-ons
            if (item.AddOns != null)
            {
                itemTotal += item.AddOns.Sum(addOn => addOn.AddOnPrice * item.Quantity);
            }

            // Add combos
            if (item.Combos != null)
            {
                itemTotal += item.Combos.Sum(combo => combo.ComboPrice * item.Quantity);
            }

            return itemTotal;
        }

        #endregion

        private string GetStatusMessage(Order.OrderStatus status)
        {
            return status switch
            {
                Order.OrderStatus.Accepted => "Order accepted and being prepared.",
                Order.OrderStatus.InTransit => "Order is on the way.",
                Order.OrderStatus.Delivered => "Order delivered.",
                Order.OrderStatus.Cancelled => "Order cancelled.",
                Order.OrderStatus.Rejected => "Order rejected.",
                _ => $"Order status updated to {status}"
            };
        }

        private Order.OrderStatus ParseStatusSafe(string statusLower)
        {
            // Map lowercase strings to your enum; extend as needed
            return statusLower switch
            {
                "accepted" => Order.OrderStatus.Accepted,
                "rejected" => Order.OrderStatus.Rejected,
                "cancelled" => Order.OrderStatus.Cancelled,
                "in_transit" => Order.OrderStatus.InTransit,
                "intransit" => Order.OrderStatus.InTransit,
                "delivered" => Order.OrderStatus.Delivered,
                "pending" => Order.OrderStatus.Pending,
                _ => Order.OrderStatus.Pending
            };
        }

        public Task NotifyCustomerOrderDelivered(string customerId, int orderId)
        {
            throw new NotImplementedException();
        }
    }
}