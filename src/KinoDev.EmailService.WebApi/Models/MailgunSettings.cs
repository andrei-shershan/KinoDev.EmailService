namespace KinoDev.EmailService.WebApi.Models
{
    public class MailgunSettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string Domain { get; set; } = string.Empty;
        public string Region { get; set; } = "US"; // US or EU
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;
    }
}
