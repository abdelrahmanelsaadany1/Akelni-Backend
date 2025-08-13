// In Services.CategoryService/OrderService.cs

using Domain.Contracts; // For IExtendedRepository
using Domain.Dtos.OrderDto;
using Domain.Entities;
using Microsoft.AspNetCore.Http; // For IHttpContextAccessor
using Microsoft.EntityFrameworkCore; // For Include/ThenInclude
using Services.Abstractions.ICategoryService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims; // For ClaimTypes
using System.Threading.Tasks;

namespace Services.CategoryService // Adjust namespace as needed
{
    public class OrderService : IOrderService
    {
        private readonly IExtendedRepository<Order> _orderRepository;
        private readonly IExtendedRepository<OrderItem> _orderItemRepository;
        private readonly IExtendedRepository<OrderItemAddOn> _orderItemAddOnRepository;
        private readonly IExtendedRepository<OrderItemCombo> _orderItemComboRepository;
        private readonly IExtendedRepository<Item> _itemRepository;
        private readonly IExtendedRepository<AddOn> _addOnRepository;
        private readonly IExtendedRepository<Combo> _comboRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IExtendedRepository<Restaurant> _restaurantRepository;
        private readonly IExtendedRepository<ItemAddOn> _itemAddOnRepo;
        private readonly IExtendedRepository<ItemCombo> _itemComboRepo;

        public OrderService(
            IExtendedRepository<Order> orderRepository,
            IExtendedRepository<OrderItem> orderItemRepository,
            IExtendedRepository<OrderItemAddOn> orderItemAddOnRepository,
            IExtendedRepository<OrderItemCombo> orderItemComboRepository,
            IExtendedRepository<Item> itemRepository,
            IExtendedRepository<AddOn> addOnRepository,
            IExtendedRepository<Combo> comboRepository,
            IExtendedRepository<Restaurant> restaurantRepository,
            IExtendedRepository<ItemAddOn> itemAddOnRepo,
            IExtendedRepository<ItemCombo> itemComboRepo,
            IHttpContextAccessor httpContextAccessor)
        {
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _orderItemAddOnRepository = orderItemAddOnRepository;
            _orderItemComboRepository = orderItemComboRepository;
            _itemRepository = itemRepository;
            _addOnRepository = addOnRepository;
            _comboRepository = comboRepository;
            _restaurantRepository = restaurantRepository;
            _httpContextAccessor = httpContextAccessor;
            _itemAddOnRepo = itemAddOnRepo;
            _itemComboRepo = itemComboRepo;

        }

        //total amount is passed
        public async Task AddOrderAsync(Order order, List<OrderItemCreateDto> orderItems, int totalAmount)
        {
            // Set CustomerId if not already set
            if (string.IsNullOrEmpty(order.CustomerId))
            {
                order.CustomerId = "2";//GetCurrentUserId();
            }

            order.Items ??= new List<OrderItem>();

            //Validate Restaurant
            var restaurant = await _restaurantRepository.GetByIdAsync(order.RestaurantId);
            if(restaurant == null)
            {
                throw new KeyNotFoundException($"Restaurant with ID {order.RestaurantId} not found.");
            }

            foreach (var itemDto in orderItems)
            {
                // Validate item
                var item = await _itemRepository.GetByIdAsync(itemDto.ItemId);
                if (item == null)
                    throw new Exception($"Item with ID {itemDto.ItemId} not found.");

                if (itemDto.Quantity <= 0 || itemDto.Quantity > 10000)
                    throw new Exception($"Quantity for item {itemDto.ItemId} is invalid.");

                var itemPrice = item.Price;
                var totalPrice = itemPrice * itemDto.Quantity;

                var orderItem = new OrderItem
                {
                    ItemId = itemDto.ItemId,
                    Quantity = itemDto.Quantity,
                    ItemPrice = itemPrice,
                    TotalPrice = totalPrice
                };

                // ✅ Get valid AddOnIds and ComboIds for this item from DB
                var validAddOnIds = await _itemAddOnRepo.FindAsync(ia => ia.ItemId == itemDto.ItemId);
                var validComboIds = await _itemComboRepo.FindAsync(ic => ic.ItemId == itemDto.ItemId);

                var allowedAddOnIds = validAddOnIds.Select(ia => ia.AddOnId).ToHashSet();
                var allowedComboIds = validComboIds.Select(ic => ic.ComboId).ToHashSet();

                // ✅ Add only valid AddOns
                var orderItemAddOns = itemDto.AddOnIds
                    .Where(addOnId => allowedAddOnIds.Contains(addOnId))
                    .Select(addOnId => new OrderItemAddOn
                    {
                        AddOnId = addOnId
                    })
                    .ToList();

                // ✅ Add only valid Combos
                var orderItemCombos = itemDto.ComboIds
                    .Where(comboId => allowedComboIds.Contains(comboId))
                    .Select(comboId => new OrderItemCombo
                    {
                        ComboId = comboId
                    })
                    .ToList();

                orderItem.AddOns = orderItemAddOns;
                orderItem.Combos = orderItemCombos;

                order.Items.Add(orderItem);
            }

            // Calculate fees and totals
            order.DeliveryFee = CalculateDeliveryFee(order.DistanceKm);
            order.PlatformFee = CalculatePlatformFee();

            // Calculate SubTotal
            order.SubTotal = totalAmount; //order.Items.Sum(item => item.TotalPrice);

            order.CreatedAt = DateTime.UtcNow;
            order.Status = Order.OrderStatus.Pending;

            await _orderRepository.AddAsync(order);
        }

        //total is calculated based on passed items
        //public async Task AddOrderAsync(Order order, List<OrderItemCreateDto> orderItems)
        //{
        //    // Set CustomerId if not already set
        //    if (string.IsNullOrEmpty(order.CustomerId))
        //    {
        //        order.CustomerId = GetCurrentUserId();
        //    }

        //    order.Items ??= new List<OrderItem>();

        //    //Validate Restaurant
        //    var restaurant = await _restaurantRepository.GetByIdAsync(order.RestaurantId);
        //    if (restaurant == null)
        //    {
        //        throw new KeyNotFoundException($"Restaurant with ID {order.RestaurantId} not found.");
        //    }

        //    foreach (var itemDto in orderItems)
        //    {
        //        // Validate item
        //        var item = await _itemRepository.GetByIdAsync(itemDto.ItemId);
        //        if (item == null)
        //            throw new Exception($"Item with ID {itemDto.ItemId} not found.");

        //        if (itemDto.Quantity <= 0 || itemDto.Quantity > 10000)
        //            throw new Exception($"Quantity for item {itemDto.ItemId} is invalid.");

        //        var itemPrice = item.Price;
        //        var totalPrice = itemPrice * itemDto.Quantity;

        //        var orderItem = new OrderItem
        //        {
        //            ItemId = itemDto.ItemId,
        //            Quantity = itemDto.Quantity,
        //            ItemPrice = itemPrice,
        //            TotalPrice = totalPrice
        //        };

        //        // ✅ Get valid AddOnIds and ComboIds for this item from DB
        //        var validAddOnIds = await _itemAddOnRepo.FindAsync(ia => ia.ItemId == itemDto.ItemId);
        //        var validComboIds = await _itemComboRepo.FindAsync(ic => ic.ItemId == itemDto.ItemId);

        //        var allowedAddOnIds = validAddOnIds.Select(ia => ia.AddOnId).ToHashSet();
        //        var allowedComboIds = validComboIds.Select(ic => ic.ComboId).ToHashSet();

        //        // ✅ Add only valid AddOns
        //        var orderItemAddOns = itemDto.AddOnIds
        //            .Where(addOnId => allowedAddOnIds.Contains(addOnId))
        //            .Select(addOnId => new OrderItemAddOn
        //            {
        //                AddOnId = addOnId
        //            })
        //            .ToList();

        //        // ✅ Add only valid Combos
        //        var orderItemCombos = itemDto.ComboIds
        //            .Where(comboId => allowedComboIds.Contains(comboId))
        //            .Select(comboId => new OrderItemCombo
        //            {
        //                ComboId = comboId
        //            })
        //            .ToList();

        //        orderItem.AddOns = orderItemAddOns;
        //        orderItem.Combos = orderItemCombos;

        //        order.Items.Add(orderItem);
        //    }

        //    // Calculate fees and totals
        //    order.DeliveryFee = CalculateDeliveryFee(order.DistanceKm);
        //    order.PlatformFee = CalculatePlatformFee();

        //    // Calculate SubTotal
        //    order.SubTotal = order.Items.Sum(item => item.TotalPrice);

        //    order.CreatedAt = DateTime.UtcNow;
        //    order.Status = Order.OrderStatus.Pending;

        //    await _orderRepository.AddAsync(order);
        //}


        public async Task<IEnumerable<OrderResponseDto>> GetAllOrdersAsync()
        {
            var orders = await _orderRepository.GetAllAsync(
                include: o => o.Include(x => x.Restaurant)
                                .Include(x => x.Items)
                                    .ThenInclude(i => i.Item)
                                .Include(x => x.Items)
                                    .ThenInclude(i => i.AddOns)
                                        .ThenInclude(a => a.AddOn)
                                .Include(x => x.Items)
                                    .ThenInclude(i => i.Combos)
                                        .ThenInclude(c => c.Combo)
                                .Include(x => x.Payment)
            );
            return orders.Select(MapToResponseDto);
        }

        public async Task<OrderResponseDto> GetOrderByIdAsync(int id)
        {
            var order = await _orderRepository.GetByIdAsync(id,
                include: o => o.Include(x => x.Restaurant)
                                .Include(x => x.Items)
                                    .ThenInclude(i => i.Item)
                                .Include(x => x.Items)
                                    .ThenInclude(i => i.AddOns)
                                        .ThenInclude(a => a.AddOn)
                                .Include(x => x.Items)
                                    .ThenInclude(i => i.Combos)
                                        .ThenInclude(c => c.Combo)
                                .Include(x => x.Payment)
            );
            if (order == null)
                throw new KeyNotFoundException($"Order with ID {id} not found");
            return MapToResponseDto(order);
        }

        public async Task<IEnumerable<OrderResponseDto>> GetOrdersByCustomerIdAsync(string customerId)
        {
            var orders = await _orderRepository.GetAllAsync(
                filter: o => o.CustomerId == customerId,
                include: o => o.Include(x => x.Restaurant)
                                .Include(x => x.Items)
                                    .ThenInclude(i => i.Item)
                                .Include(x => x.Items)
                                    .ThenInclude(i => i.AddOns)
                                        .ThenInclude(a => a.AddOn)
                                .Include(x => x.Items)
                                    .ThenInclude(i => i.Combos)
                                        .ThenInclude(c => c.Combo)
                                .Include(x => x.Payment),
                orderBy: o => o.OrderByDescending(x => x.CreatedAt)
            );
            return orders.Select(MapToResponseDto);
        }

        public async Task<IEnumerable<OrderResponseDto>> GetOrdersByRestaurantIdAsync(int restaurantId)
        {
            var orders = await _orderRepository.GetAllAsync(
                filter: o => o.RestaurantId == restaurantId,
                include: o => o.Include(x => x.Restaurant)
                                .Include(x => x.Items)
                                    .ThenInclude(i => i.Item)
                                .Include(x => x.Items)
                                    .ThenInclude(i => i.AddOns)
                                        .ThenInclude(a => a.AddOn)
                                .Include(x => x.Items)
                                    .ThenInclude(i => i.Combos)
                                        .ThenInclude(c => c.Combo)
                                .Include(x => x.Payment),
                orderBy: o => o.OrderByDescending(x => x.CreatedAt)
            );
            return orders.Select(MapToResponseDto);
        }

        public async Task<IEnumerable<OrderResponseDto>> GetOrdersByStatusAsync(Order.OrderStatus status)
        {
            var orders = await _orderRepository.GetAllAsync(
                filter: o => o.Status == status,
                include: o => o.Include(x => x.Restaurant)
                                .Include(x => x.Items)
                                    .ThenInclude(i => i.Item)
                                .Include(x => x.Items)
                                    .ThenInclude(i => i.AddOns)
                                        .ThenInclude(a => a.AddOn)
                                .Include(x => x.Items)
                                    .ThenInclude(i => i.Combos)
                                        .ThenInclude(c => c.Combo)
                                .Include(x => x.Payment),
                orderBy: o => o.OrderByDescending(x => x.CreatedAt)
            );
            return orders.Select(MapToResponseDto);
        }

        public async Task UpdateOrderStatusAsync(int id, Order.OrderStatus status)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
                throw new KeyNotFoundException($"Order with ID {id} not found");
            order.Status = status;
            await _orderRepository.UpdateAsync(order);
        }


        public async Task DeleteOrderAsync(int id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
                throw new KeyNotFoundException($"Order with ID {id} not found");
            if (order.Status != Order.OrderStatus.Pending)
                throw new InvalidOperationException("Only pending orders can be deleted");
            await _orderRepository.DeleteAsync(id);
        }

        public async Task<IEnumerable<OrderResponseDto>> GetCurrentUserOrdersAsync()
        {
            var userId = GetCurrentUserId();
            return await GetOrdersByCustomerIdAsync(userId);
        }

        public async Task<decimal> CalculateOrderTotalAsync(int orderId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId,
                include: o => o.Include(x => x.Items)
                                .ThenInclude(i => i.Item)
                                .Include(x => x.Items)
                                .ThenInclude(i => i.AddOns)
                                    .ThenInclude(a => a.AddOn)
                                .Include(x => x.Items)
                                .ThenInclude(i => i.Combos)
                                    .ThenInclude(c => c.Combo)
            );
            if (order == null)
                throw new KeyNotFoundException($"Order with ID {orderId} not found");

            decimal total = 0;
            foreach (var orderItem in order.Items)
            {
                total += orderItem.Item.Price * orderItem.Quantity;
                total += orderItem.AddOns?.Sum(a => a.AddOn.AdditionalPrice * orderItem.Quantity) ?? 0;
                total += orderItem.Combos?.Sum(c => c.Combo.ComboPrice * orderItem.Quantity) ?? 0;
            }
            total += order.DeliveryFee + order.PlatformFee;
            return total;
        }

        #region Private Helper Methods

        private string GetCurrentUserId()
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("User is not authenticated");
            return userId;
        }

        private decimal CalculateDeliveryFee(double distanceKm)
        {
            decimal baseFee = 2.50m;
            decimal perKmFee = 0.50m;
            return baseFee + (decimal)(distanceKm * (double)perKmFee);
        }

        private decimal CalculatePlatformFee()
        {
            return 1.50m;
        }

        private OrderResponseDto MapToResponseDto(Order order)
        {
            return new OrderResponseDto
            {
                Id = order.Id,
                CreatedAt = order.CreatedAt,
                Status = order.Status,
                SubTotal = order.SubTotal,
                DeliveryFee = order.DeliveryFee,
                PlatformFee = order.PlatformFee,
                DistanceKm = order.DistanceKm,
                CustomerId = order.CustomerId,
                RestaurantId = order.RestaurantId,
                RestaurantName = order.Restaurant?.Name,
                Items = order.Items?.Select(MapToOrderItemResponseDto).ToList() ?? new List<OrderItemResponseDto>(),
                Payment = order.Payment != null ? new PaymentResponseDto
                {
                    Id = order.Payment.Id,
                    StripePaymentIntentId = order.Payment.StripePaymentIntentId,
                    Amount = order.Payment.Amount,
                    PaidAt = order.Payment.PaidAt,
                    OrderId = order.Payment.OrderId
                } : null
            };
        }

        private OrderItemResponseDto MapToOrderItemResponseDto(OrderItem orderItem)
        {
            var itemTotal = orderItem.Item.Price * orderItem.Quantity;
            itemTotal += orderItem.AddOns?.Sum(a => a.AddOn.AdditionalPrice * orderItem.Quantity) ?? 0;
            itemTotal += orderItem.Combos?.Sum(c => c.Combo.ComboPrice * orderItem.Quantity) ?? 0;

            return new OrderItemResponseDto
            {
                Id = orderItem.Id,
                OrderId = orderItem.OrderId,
                ItemId = orderItem.ItemId,
                ItemName = orderItem.Item?.Name,
                ItemPrice = orderItem.Item?.Price ?? 0,
                Quantity = orderItem.Quantity,
                TotalPrice = itemTotal,
                AddOns = orderItem.AddOns?.Select(a => new OrderItemAddOnResponseDto
                {
                    Id = a.Id,
                    OrderItemId = a.OrderItemId,
                    AddOnId = a.AddOnId,
                    AddOnName = a.AddOn?.Name,
                    AddOnPrice = a.AddOn?.AdditionalPrice ?? 0
                }).ToList() ?? new List<OrderItemAddOnResponseDto>(),
                Combos = orderItem.Combos?.Select(c => new OrderItemComboResponseDto
                {
                    Id = c.Id,
                    OrderItemId = c.OrderItemId,
                    ComboId = c.ComboId,
                    ComboName = c.Combo?.Name,
                    ComboPrice = c.Combo?.ComboPrice ?? 0
                }).ToList() ?? new List<OrderItemComboResponseDto>()
            };
        }


        //public IActionResult CreateCheckoutSession(int amount)
        //{
        //    //var domain = "http://localhost:4200";
        //    var domain = "https://localhost:7045";
        //    var currency = "egp";
        //    var successUrl = domain + "/api/Orders/success";
        //    var cancelUrl = domain + "/api/Orders/cancel";
        //    //var successUrl = domain + "/success";
        //    //var cancelUrl = domain + "/cancel";
        //    StripeConfiguration.ApiKey = _stripeSettings.SecretKey;

        //    var options = new SessionCreateOptions
        //    {
        //        PaymentMethodTypes = new List<string> { "card" },
        //        LineItems = new List<SessionLineItemOptions>
        //    {
        //        new SessionLineItemOptions
        //        {
        //            PriceData = new SessionLineItemPriceDataOptions
        //            {
        //                Currency = currency,
        //                UnitAmount = Convert.ToInt32(amount) * 100,
        //                ProductData = new SessionLineItemPriceDataProductDataOptions
        //                {
        //                    Name = "Total Fees",
        //                }
        //            },
        //           Quantity = 1
        //        },
        //    }
        //        ,
        //        Mode = "payment",
        //        SuccessUrl = successUrl,
        //        CancelUrl = cancelUrl,
        //    };

        //    var service = new SessionService();
        //    var session = service.Create(options);

        //    return Ok(new { url = session.Url, id = session.Id, amount = amount });
        //}

        #endregion
    }
}