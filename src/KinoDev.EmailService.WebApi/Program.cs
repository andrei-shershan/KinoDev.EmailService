using KinoDev.EmailService.WebApi.Models;
using KinoDev.EmailService.WebApi.Services;
using KinoDev.EmailService.WebApi.Services.Abstractions;
using KinoDev.Shared.Models;
using KinoDev.Shared.Services;
using KinoDev.Shared.Services.Abstractions;

namespace KinoDev.EmailService.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            ConfigureSettings(builder);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var messageBrokerName = builder.Configuration.GetValue<string>("MessageBrokerName");
            if (messageBrokerName == "RabbitMQ")
            {
                builder.Services.AddSingleton<IMessageBrokerService, RabbitMQService>();
            }
            else if (messageBrokerName == "AzureServiceBus")
            {
                builder.Services.AddSingleton<IMessageBrokerService, AzureServiceBusService>();
            }
            else
            {
                throw new InvalidOperationException("Invalid MessageBrokerName configuration value.");
            }

            // TODO: Move to coinfiguration constants
            var emailServiceName = builder.Configuration.GetValue<string>("EmailServiceName");
            if (emailServiceName == "Mailgun")
            {
                builder.Services.AddScoped<IEmailSenderService, MailgunEmailSenderService>();
            }
            else if (emailServiceName == "Brevo")
            {
                builder.Services
                    .AddScoped<IEmailSenderService, BrevoEmailService>()
                    .AddHttpClient<BrevoEmailService>();
            }
            else
            {
                throw new ArgumentException("Invalid Email Service Name specified in configuration.");
            }

            builder.Services.AddScoped<IEmailGenerator, EmailGenerator>();
            builder.Services
                .AddScoped<IFileService, FileService>()
                .AddHttpClient<IFileService, FileService>();

            builder.Services.AddHostedService<MessagingSubscriber>();

            builder.Services.AddHealthChecks();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            var disableHttpsRedirection = builder.Configuration.GetValue<bool>("DisableHttpsRedirection");
            if (!disableHttpsRedirection)
            {
                app.UseHttpsRedirection();
            }

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }

        private static void ConfigureSettings(WebApplicationBuilder builder)
        {
            var rabbitMqSection = builder.Configuration.GetSection("RabbitMq");
            var azureServiceBusSection = builder.Configuration.GetSection("AzureServiceBus");
            var messageBrokerSection = builder.Configuration.GetSection("MessageBroker");
            var azureStorageSection = builder.Configuration.GetSection("AzureStorage");
            var mailgunSection = builder.Configuration.GetSection("Mailgun");
            var brevoSection = builder.Configuration.GetSection("Brevo");

            var rabbitMqSettings = rabbitMqSection.Get<RabbitMqSettings>();
            var azureServiceBusSettings = azureServiceBusSection.Get<AzureServiceBusSettings>();
            var messageBrokerSettings = messageBrokerSection.Get<MessageBrokerSettings>();
            var azureStorageSettings = azureStorageSection.Get<AzureStorageSettigns>();
            if (
                messageBrokerSettings == null
                || azureStorageSettings == null
            )
            {
                throw new ArgumentNullException("Configuration settings are not properly configured.");
            }

            if (rabbitMqSettings == null && azureServiceBusSettings == null)
            {
                throw new ArgumentNullException("RabbitMQ or Azure Service Bus settings are not properly configured.");
            }

            if (mailgunSection == null
            || mailgunSection == null)
            {
                throw new ArgumentNullException("Email service settings are not properly configured.");
            }

            builder.Services.Configure<RabbitMqSettings>(rabbitMqSection);
            builder.Services.Configure<AzureServiceBusSettings>(azureServiceBusSection);
            builder.Services.Configure<MessageBrokerSettings>(messageBrokerSection);
            builder.Services.Configure<BrevoSettings>(brevoSection);
            builder.Services.Configure<MailgunSettings>(mailgunSection);
            builder.Services.Configure<AzureStorageSettigns>(azureStorageSection);
        }
    }
}
