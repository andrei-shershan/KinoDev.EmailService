using System;
using System.Net;
using System.Threading.Tasks;
using KinoDev.EmailService.WebApi.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestSharp;
using RestSharp.Authenticators;

namespace KinoDev.EmailService.WebApi.Services
{
    public class MailgunEmailSenderService : IEmailSenderService
    {
        private readonly MailgunSettings _mailgunSettings;
        private readonly ILogger<MailgunEmailSenderService> _logger;

        public MailgunEmailSenderService(
            IOptions<MailgunSettings> mailgunSettings,
            ILogger<MailgunEmailSenderService> logger)
        {
            _mailgunSettings = mailgunSettings.Value;
            _logger = logger;
        }

        public async Task<bool> SendAsync(string to, string subject, string body, bool isHtml = true)
        {
            try
            {
                var baseUrl = _mailgunSettings.Region.ToUpper() == "EU" 
                    ? "https://api.eu.mailgun.net/v3" 
                    : "https://api.mailgun.net/v3";
                
                // Create RestClient with Mailgun API base URL and authentication
                var options = new RestClientOptions(baseUrl)
                {
                    Authenticator = new HttpBasicAuthenticator("api", _mailgunSettings.ApiKey)
                };
                
                var client = new RestClient(options);
                
                // Create request
                var request = new RestRequest($"{_mailgunSettings.Domain}/messages");
                
                // Add required parameters
                request.AddParameter("from", $"{_mailgunSettings.FromName} <{_mailgunSettings.FromEmail}>");
                request.AddParameter("to", to);
                request.AddParameter("subject", subject);
                
                // Add either HTML or text based on the isHtml flag
                if (isHtml)
                {
                    request.AddParameter("html", body);
                }
                else
                {
                    request.AddParameter("text", body);
                }
                
                // Execute the request
                var response = await client.ExecutePostAsync(request);
                
                // Log the result and return success/failure
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
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while sending email to {Recipient}", to);
                return false;
            }
        }
    }
}
