using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace ShopFlow.NotificationService.Infrastructure.Hubs
{
    public class NotificationHub : Hub
    {
        //Called when client connects
        public override async Task OnConnectedAsync()
        {
            await Clients.Caller.SendAsync("Connected", $"Connected to NotificationHub with ConnectionId: {Context.ConnectionId}");

            await base.OnConnectedAsync();
        }

        //Client calls this to join a order group. Group = all connections watching the same order

        public async Task JoinOrderGroup(string orderId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"order-{orderId}");

            await Clients.Caller.SendAsync("JoinedGroup", $"Joined group for order: {orderId}");
        }

        // Client calls this to leave an order group
        public async Task LeaveOrderGroup(string orderId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"order-{orderId}");
        }
    }
}
