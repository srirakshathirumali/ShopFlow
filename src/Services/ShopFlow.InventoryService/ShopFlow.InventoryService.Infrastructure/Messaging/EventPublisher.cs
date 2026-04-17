using MassTransit;
using ShopFlow.InventoryService.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShopFlow.InventoryService.Infrastructure.Messaging
{
    public class EventPublisher : IEventPublisher
    {
        private readonly IPublishEndpoint _publishEndpoint;
        public EventPublisher(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }
        public async Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class
        {
            await _publishEndpoint.Publish(message, cancellationToken);
        }
    }
}
