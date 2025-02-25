using EventManagement.BusinessLogic.Resources;
using EventManagement.BusinessLogic.Services.v1.Abstractions;
using EventManagement.DataAccess.ViewModels.ApiObjects;
using EventManagement.Utilities.Email;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventManagement.API.Controllers.v1
{
    [ApiController]
    [Produces("application/json")]
    [Route("api/v" + EventManagement.BusinessLogic.Version.Value + "/[controller]")]
    public class PaymentController : BaseController
    {
        private readonly IPaymentService _paymentService;
        private readonly ICustomerServices _customerServices;
        public PaymentController(IEmailService emailService, IPaymentService paymentService, ICustomerServices customerServices) : base(emailService)
        {
            _paymentService = paymentService;
            _customerServices = customerServices;
        }

        //[Route("session")]
        //[HttpPost]
        //[Authorize]
        //public async Task<IActionResult> CreateCheckoutSession(CreateSessionInput input)
        //{
        //    return await ExecuteAsync(() => _paymentService.CreateSession(input), Resource.SUCCESS);
        //}

        [Route("order/{orderId}")]
        [HttpGet]
        public async Task<IActionResult> GetOrderDetails([FromRoute] string orderId, [FromQuery] long organizationId, [FromQuery] string successUrl, [FromQuery] string cancelUrl)
        {
           var orderDetails =  await _paymentService.GetOrderDetails(orderId, organizationId);
            if (orderDetails != null)
            {
                return Redirect(successUrl);
            }
            return Redirect(cancelUrl);
        }

        [Route("unpaid/{orderId}")]
        [HttpGet]
        public async Task<IActionResult> PayUnpaidAmount([FromRoute] string orderId, [FromQuery] long organizationId, [FromQuery] string successUrl, [FromQuery] string cancelUrl)
        {
            var orderDetails = await _paymentService.PayUnpaidAmount(orderId, organizationId);
            if (orderDetails != null)
            {
                return Redirect(successUrl);
            }
            return Redirect(cancelUrl);
        }

        [Route("order/{orderId}/penalty")]
        [HttpGet]
        public async Task<IActionResult> GetPenalitiesDetails([FromRoute] string orderId, [FromQuery] long organizationId, [FromQuery] string successUrl, [FromQuery] string cancelUrl)
        {
            var orderDetails = await _paymentService.GetPenaltyDetails(orderId, organizationId);
            if (orderDetails != null)
            {
                return Redirect(successUrl);
            }
            return Redirect(cancelUrl);
        }


        [Route("customer/wallet/{orderId}")]
        [HttpGet]
        public async Task<IActionResult> GetCustomerWalletDetails([FromRoute] string orderId, [FromQuery] long organizationId, [FromQuery] string successUrl, [FromQuery] string cancelUrl)
        {
            var walletDetails = await _paymentService.GetCustomerWalletDetails(orderId, organizationId);
            if (walletDetails != null)
            {
                return Redirect(successUrl);
            }
            return Redirect(cancelUrl);
        }

        [Route("save")]
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SavePaymentDetail([FromBody] TransactionDetailInput input)
        {
            return await ExecuteAsync(() => _paymentService.SavePaymentDetail(input), Resource.ADD_TRANSCTION_DETAIL);
        }

        [Route("booking")]
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateBooking(CreateBookingInput input)
        {
            return await ExecuteAsync(() => _paymentService.CreateBooking(input), Resource.SUCCESS);
        }

        [HttpPut]
        [Authorize]
        [Route("status/update")]
        public async Task<IActionResult> StatusUpdate([FromBody] StatusUpdateInput input)
        {
            return await ExecuteAsync(() => _paymentService.UpdatePaymentStatus(input), Resource.AMOUNT_UPDATED);
        }
    }
}
