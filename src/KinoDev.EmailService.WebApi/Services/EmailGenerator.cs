using System.Text;
using KinoDev.EmailService.WebApi.Models;
using KinoDev.EmailService.WebApi.Services.Abstractions;
using KinoDev.Shared.DtoModels.Orders;
using KinoDev.Shared.Services;
using Microsoft.Extensions.Options;

namespace KinoDev.EmailService.WebApi.Services
{
    public class EmailGenerator : IEmailGenerator
    {
        private readonly IMessageBrokerService _messageBrokerService;
        private readonly MessageBrokerSettings _messageBrokerSettings;
        private readonly IEmailSenderService _emailSenderService;

        private readonly AzureStorageSettigns _azureStorageSettigns;
        private readonly ILogger<EmailGenerator> _logger;

        public EmailGenerator(
            IMessageBrokerService messageBrokerService,
            IOptions<MessageBrokerSettings> messageBrokerSettings,
            IEmailSenderService emailSenderService,
            ILogger<EmailGenerator> logger,
            IOptions<AzureStorageSettigns> azureStorageSettigns)
        {
            _messageBrokerService = messageBrokerService;
            _messageBrokerSettings = messageBrokerSettings.Value;
            _azureStorageSettigns = azureStorageSettigns.Value;
            _emailSenderService = emailSenderService;
            _logger = logger;
        }

        public async Task GenerateOrderCompletedEmail(OrderSummary orderSummary)
        {
            var subject = $"Your order has been completed!";

            var strBuilder = new StringBuilder();
            foreach (var ticket in orderSummary.Tickets.OrderBy(t => t.Row).ThenBy(t => t.Number))
            {
                strBuilder.AppendLine($"<p>row: {ticket.Row}, number: {ticket.Number}</p>");
            }
            var seats = strBuilder.ToString();

            var body = $@"
                            <h1>Your order is completed!</h1>
                            <p>Dear {orderSummary.Email},</p>
                            <p>We're pleased to inform you that your order has been completed.</p>
                            <p>Order Details:</p>
                            <p>{orderSummary.ShowTimeSummary.Movie.Name}</p>
                            <p>{orderSummary.ShowTimeSummary.Hall.Name}</p>
                            <p>{orderSummary.ShowTimeSummary.Time.ToString()}</p>
                            <p>Total cost: <strong>{orderSummary.Cost}</strong></p>"

                + seats

                + $@"
                            <p>You can find your order details in the attached file.</p>
                            <p>Thank you for choosing our service!</p>
                            <p>Regards,<br>KinoDev Team</p>
                            ";

            var attachmentUrl = $"{_azureStorageSettigns.BaseUrl}/{_azureStorageSettigns.StorageAccount}/{orderSummary.FileUrl}";

            var result = await _emailSenderService.SendAsync(orderSummary.Email, subject, body, attachmentUrl: attachmentUrl);
            if (result)
            {
                await _messageBrokerService.PublishAsync(
                    orderSummary,
                    _messageBrokerSettings.Topics.EmailSent
                    );
            }
            else
            {
                _logger.LogError("Failed to send email to {Email}", orderSummary.Email);
            }
        }
    }
}