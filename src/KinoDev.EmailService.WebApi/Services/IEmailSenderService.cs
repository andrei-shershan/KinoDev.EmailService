using System.Threading.Tasks;

namespace KinoDev.EmailService.WebApi.Services
{
    public interface IEmailSenderService
    {
        /// <summary>
        /// Sends an email message
        /// </summary>
        /// <param name="to">Recipient email address</param>
        /// <param name="subject">Email subject</param>
        /// <param name="body">Email body content (can be HTML)</param>
        /// <param name="isHtml">Indicates if the body content is HTML</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task<bool> SendAsync(string to, string subject, string body, bool isHtml = true, string attachmentUrl = null);
    }
}
