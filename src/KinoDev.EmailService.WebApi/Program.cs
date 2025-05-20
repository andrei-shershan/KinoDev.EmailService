using KinoDev.EmailService.WebApi.Models;
using KinoDev.EmailService.WebApi.Services;
using KinoDev.Shared.Models;
using KinoDev.Shared.Services;

namespace KinoDev.EmailService.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var rabbitMqSettings = builder.Configuration.GetSection("RabbitMq").Get<RabbitMqSettings>();
            var messageBrokerSettings = builder.Configuration.GetSection("MessageBroker").Get<MessageBrokerSettings>();
            if (rabbitMqSettings == null
            || messageBrokerSettings == null)
            {
                throw new ArgumentNullException("Config settings are not configured.");
            }       

            var azureStorageSettings = builder.Configuration.GetSection("AzureStorage").Get<AzureStorageSettigns>();
            if (azureStorageSettings == null)
            {
                throw new ArgumentNullException("Azure Storage settings are not configured.");
            }     
            
            builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection("RabbitMq"));
            builder.Services.Configure<MessageBrokerSettings>(builder.Configuration.GetSection("MessageBroker"));
            builder.Services.Configure<MailgunSettings>(builder.Configuration.GetSection("Mailgun"));
            builder.Services.Configure<AzureStorageSettigns>(builder.Configuration.GetSection("AzureStorage"));

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();            
            builder.Services.AddSingleton<IMessageBrokerService, RabbitMQService>();
            
            // Register the EmailSenderService
            builder.Services.AddSingleton<IEmailSenderService, MailgunEmailSenderService>();
            builder.Services.AddTransient<IEmailGenerator, EmailGenerator>();

            builder.Services.AddHostedService<MessagingSubscriber>();


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            //app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
