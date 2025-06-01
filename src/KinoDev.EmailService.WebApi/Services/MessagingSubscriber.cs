using KinoDev.EmailService.WebApi.Models;
using KinoDev.EmailService.WebApi.Services.Abstractions;
using KinoDev.Shared.DtoModels.Orders;
using KinoDev.Shared.Services;
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
            if (string.IsNullOrWhiteSpace(_messageBrokerSettings.Topics?.OrderFileUrlAdded) ||
               string.IsNullOrWhiteSpace(_messageBrokerSettings.Queues?.OrderFileUrlAdded))
            {
                _logger.LogError("OrderFileUrlAdded topic or queue is not configured properly.");
                return Task.CompletedTask;
            }

            return _messageBrokerService.SubscribeAsync(
                _messageBrokerSettings.Topics.OrderFileUrlAdded,
                _messageBrokerSettings.Queues.OrderFileUrlAdded,
                async (message) =>
            {
                try
                {
                    _logger.LogInformation("Received order completed message: {Message}", message);

                    var orderData = JsonSerializer.Deserialize<OrderSummary>(message);
                    if (!string.IsNullOrEmpty(orderData?.Email))
                    {
                        await _emailGenerator.GenerateOrderCompletedEmail(orderData);
                    }
                    else
                    {
                        _logger.LogError("Received order completed message without valid email: {Message}", message);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing order completed message: {Message}", message);
                }
            });
        }
    }
}