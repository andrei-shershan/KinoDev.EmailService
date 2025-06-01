using KinoDev.EmailService.WebApi.Models;
using KinoDev.EmailService.WebApi.Services;
using KinoDev.EmailService.WebApi.Services.Abstractions;
using KinoDev.Shared.Models;
using KinoDev.Shared.Services;

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
            builder.Services.AddSingleton<IMessageBrokerService, RabbitMQService>();

            builder.Services.AddSingleton<IEmailSenderService, MailgunEmailSenderService>();
            builder.Services.AddTransient<IEmailGenerator, EmailGenerator>();

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
            var messageBrokerSection = builder.Configuration.GetSection("MessageBroker");
            var azureStorageSection = builder.Configuration.GetSection("AzureStorage");
            var mailgunSection = builder.Configuration.GetSection("Mailgun");

            var rabbitMqSettings = rabbitMqSection.Get<RabbitMqSettings>();
            var messageBrokerSettings = messageBrokerSection.Get<MessageBrokerSettings>();
            var azureStorageSettings = azureStorageSection.Get<AzureStorageSettigns>();
            if (
                rabbitMqSettings == null
                || messageBrokerSettings == null
                || azureStorageSettings == null
            )
            {
                throw new ArgumentNullException("Configuration settings are not properly configured.");
            }

            builder.Services.Configure<RabbitMqSettings>(rabbitMqSection);
            builder.Services.Configure<MessageBrokerSettings>(messageBrokerSection);
            builder.Services.Configure<MailgunSettings>(mailgunSection);
            builder.Services.Configure<AzureStorageSettigns>(azureStorageSection);
        }
    }
}
