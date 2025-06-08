using KinoDev.EmailService.WebApi.Models;
using KinoDev.EmailService.WebApi.Services.Abstractions;
using Microsoft.Extensions.Options;
using RestSharp;
using RestSharp.Authenticators;

namespace KinoDev.EmailService.WebApi.Services
{
    public class MailgunEmailSenderService : IEmailSenderService
    {
        private readonly MailgunSettings _mailgunSettings;
        private readonly IFileService _fileService;
        private readonly ILogger<MailgunEmailSenderService> _logger;

        public MailgunEmailSenderService(
            IFileService fileService,
            IOptions<MailgunSettings> mailgunSettings,
            ILogger<MailgunEmailSenderService> logger)
        {
            _fileService = fileService;
            _mailgunSettings = mailgunSettings.Value;
            _logger = logger;
        }

        public async Task<bool> SendAsync(string to, string subject, string body, bool isHtml = true, string? attachmentUrl = null)
        {
            try
            {
                var options = new RestClientOptions(_mailgunSettings.BaseUrl)
                {
                    Authenticator = new HttpBasicAuthenticator("api", _mailgunSettings.ApiKey)
                };

                var client = new RestClient(options);
                var request = new RestRequest($"{_mailgunSettings.Domain}/messages");

                // Add required parameters
                request.AddParameter("from", $"{_mailgunSettings.FromName} <{_mailgunSettings.FromEmail}>");
                request.AddParameter("to", to);
                request.AddParameter("subject", subject);

                try
                {
                    if (!string.IsNullOrWhiteSpace(attachmentUrl))
                    {
                        var pdfBytes = await _fileService.GetFileBytesAsync(attachmentUrl);

                        request.AddFile("attachment", pdfBytes, attachmentUrl.Split('/').Last(), "application/pdf");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while downloading the attachment from {AttachmentUrl}", attachmentUrl);
                }

                // Add either HTML or text based on the isHtml flag
                request.AddParameter(isHtml ? "html" : "text", body);

                // Execute the request
                var response = await client.ExecutePostAsync(request);
                if (response.IsSuccessful)
                {
                    _logger.LogInformation("Email sent successfully to {Recipient}. Response: {Response}",
                        to, response.Content);
                    return true;
                }
                else
                {
                    _logger.LogError("Failed to send email to {Recipient}. Status: {Status}, Response: {Response}",
                        to, response.StatusCode, response.Content);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while sending email to {Recipient}", to);
            }

            return false;
        }
    }
}
