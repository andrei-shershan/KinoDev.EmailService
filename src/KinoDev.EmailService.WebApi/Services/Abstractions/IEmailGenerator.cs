using KinoDev.Shared.DtoModels.Orders;

namespace KinoDev.EmailService.WebApi.Services.Abstractions
{
    public interface IEmailGenerator
    {
        Task GenerateOrderCompletedEmail(OrderSummary orderSummary);
    }
}