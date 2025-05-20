namespace KinoDev.EmailService.WebApi.Models
{
    public class MailgunSettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string Domain { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;

        public string BaseUrl { get; set; } = string.Empty;

    }
}
