namespace KinoDev.EmailService.WebApi.Models
{
    public class MessageBrokerSettings
    {
        public Queues? Queues { get; set; }
    }

    public class Queues
    {
        public string? OrderFileUrlAdded { get; set; }
        public string? OrderFileCreated { get; set; }

        public string? EmailSent { get; set; }
    }
}