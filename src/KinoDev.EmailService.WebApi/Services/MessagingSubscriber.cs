using KinoDev.EmailService.WebApi.Models;
using KinoDev.Shared.DtoModels.Orders;
using KinoDev.Shared.Services;
using Microsoft.Extensions.Options;
using System.Text;
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
            return _messageBrokerService.SubscribeAsync(
                _messageBrokerSettings.Topics.OrderFileUrlAdded,
                _messageBrokerSettings.Queues.OrderFileUrlAdded,
                async (message) =>
            {
                try
                {
                    _logger.LogInformation("Received order completed message: {Message}", message);

                    // Parse the message (this would depend on your actual message format)
                    // For example, assuming it's a JSON string with 'email', 'orderId', etc.
                    var orderData = JsonSerializer.Deserialize<OrderSummary>(message);
                    if (orderData != null && !string.IsNullOrEmpty(orderData.Email))
                    {
                        await _emailGenerator.GenerateOrderCompletedEmail(orderData);
                    }
                    else
                    {
                        _logger.LogWarning("Received order completed message without valid email: {Message}", message);
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