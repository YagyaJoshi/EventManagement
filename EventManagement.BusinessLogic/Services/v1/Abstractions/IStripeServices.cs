using EventManagement.DataAccess.ViewModels.ApiObjects;
using EventManagement.DataAccess.ViewModels.Dtos;

namespace EventManagement.BusinessLogic.Services.v1.Abstractions
{
    public interface IStripeServices
    {
        SubscriptionDto CreateSubscription(string sessionId);

        Task<CheckoutSessionDto> CreateCheckoutSession(string email, string priceId, long subscriptionPlanId, string successUrl, string cancelUrl);

        DateTime? CancelSubscription(string subscriptionId);

    }
}
