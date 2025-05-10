using KinoDev.EmailService.WebApi.Models;
using KinoDev.Shared.Services;
using Microsoft.Extensions.Options;

namespace KinoDev.EmailService.WebApi.Services
{
    public class MessagingSubscriber : BackgroundService
    {
        private readonly IMessageBrokerService _messageBrokerService;
        private readonly MessageBrokerSettings _messageBrokerSettings;

        public MessagingSubscriber(
            IMessageBrokerService messageBrokerService,
            IOptions<MessageBrokerSettings> messageBrokerSettings)
        {
            _messageBrokerService = messageBrokerService;
            _messageBrokerSettings = messageBrokerSettings.Value;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return _messageBrokerService.SubscribeAsync(
                _messageBrokerSettings.Topics.OrderCompleted,
                _messageBrokerSettings.Queues.EmailServiceOrderCompleted,
                async (message) =>
            {
                // Handle the message here. For example, you can log it or process it.
            });
        }
    }
}