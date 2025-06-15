using KinoDev.EmailService.WebApi.Models.RequestModels;
using KinoDev.EmailService.WebApi.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace KinoDev.EmailService.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmailsController : ControllerBase
    {
        private readonly IEmailSenderService _emailSenderService;

        public EmailsController(IEmailSenderService emailSenderService)
        {
            _emailSenderService = emailSenderService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendEmail([FromBody] EmailRequest emailRequest)
        {
            if (emailRequest == null || string.IsNullOrWhiteSpace(emailRequest.To) || string.IsNullOrWhiteSpace(emailRequest.Subject) || string.IsNullOrWhiteSpace(emailRequest.Body))
            {
                return BadRequest("Invalid email request.");
            }

            var response = await _emailSenderService.SendAsync(emailRequest.To, emailRequest.Subject, emailRequest.Body);
            if (response)
            {
                return Ok("Email sent successfully.");
            }

            return BadRequest("Failed to send email.");
        }

        [HttpGet("test")]
        public IActionResult TestEmail()
        {
            // This endpoint can be used to test the email service
            return Ok("Email service is running. " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        }
    }
}
