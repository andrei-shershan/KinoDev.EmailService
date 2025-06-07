using KinoDev.EmailService.WebApi.Models;
using KinoDev.EmailService.WebApi.Services.Abstractions;
using KinoDev.Shared.DtoModels.Orders;
using KinoDev.Shared.Services.Abstractions;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace KinoDev.EmailService.WebApi.Services
{
    public class MessagingSubscriber : BackgroundService
    {
        private readonly IMessageBrokerService _messageBrokerService;
        private readonly MessageBrokerSettings _messageBrokerSettings;
        private readonly IEmailGenerator _emailGenerator;
        private readonly ILogger<MessagingSubscriber> _logger;

        public MessagingSubscriber(
            IMessageBrokerService messageBrokerService,
            IOptions<MessageBrokerSettings> messageBrokerSettings,
            IEmailGenerator emailGenerator,
            ILogger<MessagingSubscriber> logger)
        {
            _messageBrokerService = messageBrokerService;
            _messageBrokerSettings = messageBrokerSettings.Value;
            _emailGenerator = emailGenerator;
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (string.IsNullOrWhiteSpace(_messageBrokerSettings.Queues?.OrderFileUrlAdded))
            {
                _logger.LogError("OrderFileUrlAdded topic or queue is not configured properly.");
                return Task.CompletedTask;
            }

            return _messageBrokerService.SubscribeAsync<OrderSummary>(
                _messageBrokerSettings.Queues.OrderFileUrlAdded,
                async (orderSummary) =>
            {
                await _emailGenerator.GenerateOrderCompletedEmail(orderSummary);
            });
        }
    }
}