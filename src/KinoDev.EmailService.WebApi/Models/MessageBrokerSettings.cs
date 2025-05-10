namespace KinoDev.EmailService.WebApi.Models
{
    public class MessageBrokerSettings
    {
        public Topics Topics { get; set; }

        public Queues Queues { get; set; }
    }

    public class Topics
    {
        public string OrderCompleted { get; set; }
        public string EmailSent { get; set; }
    }

    public class Queues
    {
        public string EmailServiceOrderCompleted { get; set; }
    }
}