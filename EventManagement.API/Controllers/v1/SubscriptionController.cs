using EventManagement.BusinessLogic.Exceptions;
using EventManagement.BusinessLogic.Resources;
using EventManagement.BusinessLogic.Services.v1.Abstractions;
using EventManagement.DataAccess.Models;
using EventManagement.DataAccess.ViewModels.ApiObjects;
using EventManagement.Utilities.Email;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventManagement.API.Controllers.v1
{
    [ApiController]
    [Produces("application/json")]
    [Route("api/v" + BusinessLogic.Version.Value + "/[controller]")]
    public class SubscriptionController : BaseController
    {
        private readonly ISubscriptionServices _subscriptionServices;
        private readonly IStripeServices _stripeServices;
        private readonly IUserServices _userServices;
        public SubscriptionController(ISubscriptionServices subscriptionServices, IStripeServices stripeServices, IUserServices userServices, IEmailService emailService) : base(emailService)
        {
            _subscriptionServices = subscriptionServices;
            _stripeServices = stripeServices;
            _userServices = userServices;
        }

        [HttpPost]
        [Authorize]
        [Route("createCheckoutSession")]
        public async Task<IActionResult> CreateCheckoutSession([FromBody] CheckoutSessionInput input)
        {
            var plan = _subscriptionServices.GetSubscriptionById(input.SubscriptionPlanId).Result;

            if (plan == null)
                throw new ServiceException(Resource.INVALID_PLAN);

            long userId = (HttpContext.Items["UserId"] as long?) ?? 0;

            var email = _userServices.GetUserById(userId).Result.Email;

            return await ExecuteAsync(() => _stripeServices.CreateCheckoutSession(email, plan.PriceId, plan.Id, input.SuccessUrl, input.CancelUrl), Resource.SUCCESS);
        }

        [HttpPost]
        [Authorize]
        [Route("createSubscription")]
        public async Task<IActionResult> CreateSubscription([FromBody] CreateSubscriptionInput input)
        {
            long organizationId = (HttpContext.Items["OrganizationId"] as long?) ?? 0;
            return await ExecuteAsync(() => _subscriptionServices.CreateSubscription(organizationId, input), Resource.ADD_SUBSCRIPTION_SUCCESS);
        }

        [HttpPut]
        [Authorize]
        [Route("{id}")]
        public async Task<IActionResult> UpdateSubscriptionplan([FromRoute]long? id,[FromBody] UpdateSubscriptionPlanInput input)
        {
            return await ExecuteAsync(() => _subscriptionServices.UpdateSubscriptionplan(id, input), Resource.UPDATE_SUBSCRIPTION_SUCCESS);
        }

        [HttpGet]
        [Authorize]
        [Route("{id}")]
        public async Task<IActionResult> GetSubscriptionById([FromRoute] long id)
        {
            return await ExecuteAsync(() => _subscriptionServices.GetSubscriptionById(id), Resource.SUCCESS);
        }

        [HttpGet]
        [Authorize]
        [Route("all")]
        public async Task<IActionResult> GetSubscriptions()
        {
            return await ExecuteAsync(() => _subscriptionServices.GetSubscriptions(), Resource.SUCCESS);
        }

        [HttpGet]
        [Route("public/all")]
        public async Task<IActionResult> GetAllSubscriptions()
        {
            return await ExecuteAsync(() => _subscriptionServices.GetSubscriptions(), Resource.SUCCESS);
        }

        [HttpDelete]
        [Authorize]
        [Route("cancel")]
        public async Task<IActionResult> CancelSubscription(CancelSubscriptionInput input)
        {
            long organizationId = (HttpContext.Items["OrganizationId"] as long?) ?? 0;
            return await ExecuteAsync(() => _subscriptionServices.CancelSubscription(organizationId, input), Resource.CANCEL_SUBSCRIPTION);
        }
    }
}
