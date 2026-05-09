using System;
using System.Collections.Generic;
using System.Text;

namespace ShopFlow.NotificationService.Application.Interfaces
{
    public interface INotificationHubService
    {
        Task SendOrderUpdateAsync(string orderId,string status,string message);
    }
}
