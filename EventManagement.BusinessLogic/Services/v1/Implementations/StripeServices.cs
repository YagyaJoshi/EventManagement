using EventManagement.BusinessLogic.Services.v1.Abstractions;
using EventManagement.DataAccess.ViewModels.Dtos;
using Microsoft.Extensions.Configuration;
using Stripe;
using Stripe.Checkout;

namespace EventManagement.BusinessLogic.Services.v1.Implementations
{
    public class StripeServices : IStripeServices
    {
        private readonly IConfiguration _configuration;
        private readonly SessionService _sessionService;
        private readonly SubscriptionService _subscriptionService;

        public StripeServices(IConfiguration configuration)
        {
            _configuration = configuration;
            StripeConfiguration.ApiKey = _configuration["AppSettings:StripeApiKey"];
            _sessionService = new SessionService();
            _subscriptionService = new SubscriptionService();
        }

        public SubscriptionDto CreateSubscription(string sessionId)
        {
            var checkoutSession = _sessionService.Get(sessionId);

            var subscriptionDto = new SubscriptionDto()
            {
                CustomerId = checkoutSession.CustomerId,
                SubscriptionId = checkoutSession.SubscriptionId,
                SubscriptionPlanId = checkoutSession.Metadata["subscriptionPlanId"]
            };

            return subscriptionDto;
        }

        public DateTime? CancelSubscription(string subscriptionId)
        {
            var options = new SubscriptionUpdateOptions
            {
                CancelAtPeriodEnd = true,
            };

            Subscription subscription =  _subscriptionService.Update(subscriptionId, options);

            return subscription.CancelAt;
        }

        public Task<CheckoutSessionDto> CreateCheckoutSession(string email, string priceId, long subscriptionPlanId, string successUrl, string cancelUrl)
        {
            var options = new SessionCreateOptions
            {
                CustomerEmail = email,
                LineItems = new List<SessionLineItemOptions>
                {
                  new SessionLineItemOptions
                  {
                    Price = priceId,
                    Quantity = 1,
                  },
                },
                Mode = "subscription",
                SuccessUrl = successUrl + "?success=true&session_id={CHECKOUT_SESSION_ID}",
                CancelUrl = cancelUrl + "?canceled=true",
                Metadata = new Dictionary<string, string>
                {
                    { "subscriptionPlanId", subscriptionPlanId.ToString() }
                },
            };

            var session =  _sessionService.Create(options);

            return Task.FromResult(new CheckoutSessionDto()
            {
                SessionId = session.Id,
                SessionUrl = session.Url,
            });
        }
    }
}
