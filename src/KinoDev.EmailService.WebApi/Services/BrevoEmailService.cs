using System.Text;
using System.Text.Json;
using KinoDev.EmailService.WebApi.Models;
using KinoDev.EmailService.WebApi.Services.Abstractions;
using Microsoft.Extensions.Options;

namespace KinoDev.EmailService.WebApi.Services;

public class BrevoEmailService : IEmailSenderService
{
    private readonly ILogger<BrevoEmailService> _logger;
    private readonly BrevoSettings _brevoSettings;
    private readonly HttpClient _httpClient;

    public BrevoEmailService(
        IOptions<BrevoSettings> brevoSettings,
        HttpClient httpClient,
        ILogger<BrevoEmailService> logger)
    {
        _brevoSettings = brevoSettings.Value;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<bool> SendAsync(string to, string subject, string body, bool isHtml = true, string? attachmentUrl = null)
    {
        try
        {
            var pdfBase64 = string.Empty;
            if (!string.IsNullOrWhiteSpace(attachmentUrl))
            {
                var pdfBytes = await _httpClient.GetByteArrayAsync(attachmentUrl);
                pdfBase64 = Convert.ToBase64String(pdfBytes);
            }

            var payload = (!string.IsNullOrWhiteSpace(pdfBase64) && !string.IsNullOrWhiteSpace(attachmentUrl))
                ? GetEmailPayloadWithAttachment(to, subject, body, pdfBase64, $"{attachmentUrl?.Split('/').Last() ?? "attachment"}.pdf")
                : GetEmailPayloaddWithoutAttachment(to, subject, body);

            _httpClient.DefaultRequestHeaders.Add("api-key", _brevoSettings.ApiKey);

            var response = await _httpClient.PostAsync(
                $"{_brevoSettings.BaseUrl}/smtp/email",
                new StringContent(payload, Encoding.UTF8, "application/json")
            );

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to send email to {Recipient}. Status Code: {StatusCode}, Error: {ErrorContent}", to, response.StatusCode, errorContent);
            }

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while sending email to {Recipient}", to);
            return false;
        }
    }

    private string GetEmailPayloaddWithoutAttachment(string to, string subject, string body)
    {
        var payload = new
        {
            sender = new { name = _brevoSettings.SenderName, email = _brevoSettings.SenderEmail },
            to = new[] { new { email = to } },
            subject,
            htmlContent = body,
        };

        return JsonSerializer.Serialize(payload);
    }

    private string GetEmailPayloadWithAttachment(string to, string subject, string body, string pdfBase64, string fileName)
    {
        var payload = new
        {
            sender = new { name = _brevoSettings.SenderName, email = _brevoSettings.SenderEmail },
            to = new[] { new { email = to } },
            subject,
            htmlContent = body,
            attachment = new[]
            {
                new
                {
                    content = pdfBase64,
                    name = fileName
                }
            }
        };

        return JsonSerializer.Serialize(payload);
    }
}