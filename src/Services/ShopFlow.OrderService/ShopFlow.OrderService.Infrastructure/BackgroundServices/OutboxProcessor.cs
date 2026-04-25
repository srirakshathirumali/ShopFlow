using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ShopFlow.Contracts.Events;
using ShopFlow.OrderService.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace ShopFlow.OrderService.Infrastructure.BackgroundServices
{
    public class OutboxProcessor : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OutboxProcessor> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromSeconds(5);

        public OutboxProcessor(IServiceProvider serviceProvider, ILogger<OutboxProcessor> logger)
        {
             _logger = logger;
            _serviceProvider = serviceProvider;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Outbox processor atarted");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessOutboxMessagesAsync(stoppingToken);
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, "Error in outbox processor");
                }
                await Task.Delay(_interval, stoppingToken);
            }
        }

        private async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
        {
            // New scope each cycle — BackgroundService is singleton but IOutboxRepository is scoped

            using var scope = _serviceProvider.CreateScope();

            var outboxRepository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();

            var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

            var messages = await outboxRepository.GetUnprocessedAsync();

            foreach (var message in messages)
            {
                try
                {
                    _logger.LogInformation("Publishing outbox message: {EventType} — Id: {Id}",message.EventType, message.Id);

                    await PublishMessageAsync(publishEndpoint,message.EventType,message.Payload,cancellationToken);

                    await outboxRepository.MarkAsProcessedAsync(message.Id);

                    _logger.LogInformation("Outbox message published: {Id}", message.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,"Failed to publish outbox message: {Id} RetryCount: {RetryCount}",message.Id, message.RetryCount);

                    await outboxRepository.IncrementRetryCountAsync(message.Id);
                }
            }

        }

        private static async Task PublishMessageAsync(IPublishEndpoint publishEndpoint,string eventType,string payload,CancellationToken cancellationToken)
        {
            if (eventType == typeof(OrderPlaced).FullName)
            {
                var @event = JsonSerializer.Deserialize<OrderPlaced>(payload)!;
                await publishEndpoint.Publish(@event, cancellationToken);
                return;
            }

            if (eventType == typeof(OrderCancelled).FullName)
            {
                var @event = JsonSerializer.Deserialize<OrderCancelled>(payload)!;
                await publishEndpoint.Publish(@event, cancellationToken);
                return;
            }

            throw new InvalidOperationException($"Unknown event type: {eventType}");
        }
    }
}
