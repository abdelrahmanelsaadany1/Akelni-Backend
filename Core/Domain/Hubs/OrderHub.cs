using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Domain.Hubs
{
    public class OrderHub : Hub
    {
        public async Task JoinChefGroup(string chefId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Chef_{chefId}");
        }
        public async Task JoinCustomerGroup(string customerId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Customer_{customerId}");
        }
        public async Task LeaveChefGroup(string chefId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Chef_{chefId}");
        }
        public async Task LeaveCustomerGroup(string customerId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Customer_{customerId}");
        }
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRoles = Context.User?.FindAll(ClaimTypes.Role)?.Select(c => c.Value).ToList();

            if(!string.IsNullOrEmpty(userId) && userRoles != null)
            {
                if(userRoles.Contains("Chef") || userRoles.Contains("chef"))
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"Chef_{userId}");
                }
                else if(userRoles.Contains("Customer") || userRoles.Contains("customer"))
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"Customer_{userId}");
                }
            }

            await base.OnConnectedAsync();
        }
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRoles = Context.User?.FindAll(ClaimTypes.Role)?.Select(c => c.Value).ToList();

            if (!string.IsNullOrEmpty(userId) && userRoles != null)
            {
                if (userRoles.Contains("Chef") || userRoles.Contains("chef"))
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Chef_{userId}");
                }
                else if (userRoles.Contains("Customer") || userRoles.Contains("customer"))
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Customer_{userId}");
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

    }
}
