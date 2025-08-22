using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace FoodCourt.Hubs
{
    [Authorize]
    public class OrderHub : Hub
    {
        public async Task JoinChefGroup(string chefId)
        {
            try
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"Chef_{chefId}");
                Console.WriteLine($"✅ Chef {chefId} joined group with connection {Context.ConnectionId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error joining chef group: {ex.Message}");
            }
        }

        public async Task JoinCustomerGroup(string customerId)
        {
            try
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"Customer_{customerId}");
                Console.WriteLine($"✅ Customer {customerId} joined group with connection {Context.ConnectionId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error joining customer group: {ex.Message}");
            }
        }

        public async Task LeaveChefGroup(string chefId)
        {
            try
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Chef_{chefId}");
                Console.WriteLine($"✅ Chef {chefId} left group");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error leaving chef group: {ex.Message}");
            }
        }

        public async Task LeaveCustomerGroup(string customerId)
        {
            try
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Customer_{customerId}");
                Console.WriteLine($"✅ Customer {customerId} left group");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error leaving customer group: {ex.Message}");
            }
        }

        public override async Task OnConnectedAsync()
        {
            try
            {
                var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRoles = Context.User?.FindAll(ClaimTypes.Role)?.Select(c => c.Value).ToList();

                Console.WriteLine($"🔗 SignalR Connection: {Context.ConnectionId}");
                Console.WriteLine($"👤 User ID: {userId}");
                Console.WriteLine($"🎭 Roles: {string.Join(", ", userRoles ?? new List<string>())}");

                if (!string.IsNullOrEmpty(userId) && userRoles != null)
                {
                    if (userRoles.Contains("Chef"))
                    {
                        await Groups.AddToGroupAsync(Context.ConnectionId, $"Chef_{userId}");
                        Console.WriteLine($"✅ Auto-joined Chef_{userId} group");
                    }
                    else if (userRoles.Contains("Customer"))
                    {
                        await Groups.AddToGroupAsync(Context.ConnectionId, $"Customer_{userId}");
                        Console.WriteLine($"✅ Auto-joined Customer_{userId} group");
                    }
                }

                await base.OnConnectedAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SignalR OnConnectedAsync error: {ex.Message}");
                Console.WriteLine($"❌ Stack trace: {ex.StackTrace}");
            }
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            try
            {
                var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRoles = Context.User?.FindAll(ClaimTypes.Role)?.Select(c => c.Value).ToList();

                Console.WriteLine($"🔌 SignalR Disconnection: {Context.ConnectionId}");
                if (exception != null)
                {
                    Console.WriteLine($"❌ Disconnect reason: {exception.Message}");
                }

                if (!string.IsNullOrEmpty(userId) && userRoles != null)
                {
                    if (userRoles.Contains("Chef"))
                    {
                        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Chef_{userId}");
                    }
                    else if (userRoles.Contains("Customer"))
                    {
                        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Customer_{userId}");
                    }
                }

                await base.OnDisconnectedAsync(exception);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SignalR OnDisconnectedAsync error: {ex.Message}");
            }
        }
    }
}