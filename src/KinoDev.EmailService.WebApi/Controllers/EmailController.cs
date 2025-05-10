using Microsoft.AspNetCore.Mvc;
using KinoDev.EmailService.WebApi.Services;
using System.Threading.Tasks;

namespace KinoDev.EmailService.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        private readonly IEmailSenderService _emailSenderService;

        public EmailController(IEmailSenderService emailSenderService)
        {
            _emailSenderService = emailSenderService;
        }

        /// <summary>
        /// Test endpoint to send an email using the configured email service
        /// </summary>
        [HttpPost("send-test")]
        public async Task<IActionResult> SendTestEmail([FromBody] TestEmailRequest request)
        {
            if (string.IsNullOrEmpty(request.To))
            {
                return BadRequest("Recipient email address is required");
            }

            var result = await _emailSenderService.SendAsync(
                request.To,
                request.Subject ?? "Test Email from KinoDev Email Service",
                request.Body ?? "This is a test email from the KinoDev Email Service.",
                request.IsHtml ?? true
            );

            if (result)
            {
                return Ok(new { success = true, message = $"Test email sent to {request.To}" });
            }
            else
            {
                return StatusCode(500, new { success = false, message = "Failed to send test email" });
            }
        }
    }

    public class TestEmailRequest
    {
        public string To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public bool? IsHtml { get; set; }
    }
}
