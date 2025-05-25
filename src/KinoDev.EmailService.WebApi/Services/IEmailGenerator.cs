using System.Text;
using KinoDev.EmailService.WebApi.Models;
using KinoDev.Shared.DtoModels.Orders;
using KinoDev.Shared.Services;
using Microsoft.Extensions.Options;

namespace KinoDev.EmailService.WebApi.Services
{
    public interface IEmailGenerator
    {
        Task GenerateOrderCompletedEmail(OrderSummary orderSummary);
    }

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
            // Prepare email content
            var subject = $"Your order #{orderSummary.Id} has been completed";

            var strBuilder = new StringBuilder();
            foreach (var ticket in orderSummary.Tickets)
            {
                strBuilder.AppendLine($"<p>row: {ticket.Row}, number: {ticket.Number}</p>");
            }
            var seats = strBuilder.ToString();

            var body = $@"
                            <h1>Order Completed</h1>
                            <p>Dear {orderSummary.Email},</p>
                            <p>We're pleased to inform you that your order #{orderSummary.Id} has been completed.</p>
                            <p>Order Details:</p>
                            <p>{orderSummary.ShowTimeSummary.Movie.Name}</p>
                            <p>{orderSummary.ShowTimeSummary.Hall.Name}</p>
                            <p>{orderSummary.ShowTimeSummary.Time.ToString()}</p>
                            <p>Total cost: <strong>{orderSummary.Cost}</strong></p>"

                + seats

                + $@"
                            <p>Thank you for choosing our service!</p>
                            <p>Regards,<br>KinoDev Team</p>
                            ";

            // Send the email
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