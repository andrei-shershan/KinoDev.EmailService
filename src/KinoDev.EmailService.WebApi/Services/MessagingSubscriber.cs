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
        private readonly IEmailSenderService _emailSenderService;
        private readonly ILogger<MessagingSubscriber> _logger;

        public MessagingSubscriber(
            IMessageBrokerService messageBrokerService,
            IOptions<MessageBrokerSettings> messageBrokerSettings,
            IEmailSenderService emailSenderService,
            ILogger<MessagingSubscriber> logger)
        {
            _messageBrokerService = messageBrokerService;
            _messageBrokerSettings = messageBrokerSettings.Value;
            _emailSenderService = emailSenderService;
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return _messageBrokerService.SubscribeAsync(
                _messageBrokerSettings.Topics.OrderCompleted,
                _messageBrokerSettings.Queues.EmailServiceOrderCompleted,
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
                        // Prepare email content
                        var subject = $"Your order #{orderData.Id} has been completed";

                        var strBuilder = new StringBuilder();
                        foreach (var ticket in orderData.Tickets)
                        {
                            strBuilder.AppendLine($"<p>row: {ticket.Row}, number: {ticket.Number}</p>");
                        }
                        var seats = strBuilder.ToString();

                        var body = $@"
                            <h1>Order Completed</h1>
                            <p>Dear {orderData.Email},</p>
                            <p>We're pleased to inform you that your order #{orderData.Id} has been completed.</p>
                            <p>Order Details:</p>
                            <p>{orderData.ShowTimeSummary.Movie.Name}</p>
                            <p>{orderData.ShowTimeSummary.Hall.Name}</p>
                            <p>{orderData.ShowTimeSummary.Time.ToString()}</p>
                            <p>Total cost: <strong>{orderData.Cost}</strong></p>"

                            + seats

                            + $@"
                            <p>Thank you for choosing our service!</p>
                            <p>Regards,<br>KinoDev Team</p>
                            ";

                        // Send the email
                        var result = await _emailSenderService.SendAsync(orderData.Email, subject, body);

                        if (result)
                        {
                            await _messageBrokerService.PublishAsync(
                                orderData.Id.ToString(),
                                _messageBrokerSettings.Topics.EmailSent
                                );
                        }
                        else
                        {
                            _logger.LogError("Failed to send order completion email to {Email} for order {OrderId}",
                                orderData.Email, orderData.Id);
                        }
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