using EventManagement.BusinessLogic.Services.v1.Abstractions;
using EventManagement.Utilities.Email;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace EventManagement.API.Controllers.v1
{
    [ApiController]
    [Produces("application/json")]
    [Route("api/v" + BusinessLogic.Version.Value + "/[controller]")]
    public class WebhookController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IEmailService _emailService;
        public WebhookController(IPaymentService paymentService, IEmailService emailService)
        {
            _paymentService = paymentService;
            _emailService = emailService;
        }

        [HttpPost]
        [Route("")]
        public async Task<IActionResult> StripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            try
            {
                var stripeEvent = EventUtility.ParseEvent(json, throwOnApiVersionMismatch: false);
                var type = stripeEvent.Type;


                if (stripeEvent.Type == Events.PayoutFailed)
                {
                    var paymentIntent = stripeEvent.Data.Object as Payout;
                    await _paymentService.SaveWebhookEvent(null, paymentIntent.DestinationId, json, type);
                    Console.WriteLine("Payout was failed!");
                    _emailService.SendEmail("narsing.m@synsoftglobal.com", "EMS Webhook - Payout failed", json);
                    _emailService.SendEmail("mahimakhore.synsoft@gmail.com", "EMS Webhook - Payout failed", json);
                }
                else if (stripeEvent.Type == Events.InvoicePaymentFailed)
                {
                    var paymentIntent = stripeEvent.Data.Object as Invoice;
                    var customerEmail = paymentIntent?.CustomerEmail;
                    var customerId = paymentIntent?.CustomerId;
                    await _paymentService.SaveWebhookEvent(paymentIntent.CustomerId, null, json, type);
                    Console.WriteLine("Invoice Payment was failed!");
                    _emailService.SendEmail("narsing.m@synsoftglobal.com", "EMS Webhook - Invoice Payment failed", json);
                    _emailService.SendEmail("mahimakhore.synsoft@gmail.com", "EMS Webhook- Invoice Payment failed", json);

                    await _paymentService.UpdateSubscriptionPlan(customerId, customerEmail);

                }
                else if (stripeEvent.Type == Events.InvoicePaid)
                {
                    var paymentIntent = stripeEvent.Data.Object as Invoice;

                    var customerId = paymentIntent?.CustomerId;
                    var subscriptionId = paymentIntent?.Lines?.Data?.FirstOrDefault()?.SubscriptionId;


                    await _paymentService.SaveWebhookEvent(paymentIntent.CustomerId, paymentIntent.SubscriptionId, json, type);
                    Console.WriteLine("Invoice Paid is success!");
                    _emailService.SendEmail("narsing.m@synsoftglobal.com", "EMS Webhook - Invoice Paid success", json);
                    _emailService.SendEmail("mahimakhore.synsoft@gmail.com", "EMS Webhook - Invoice Paid success", json);
                    await _paymentService.UpdateSubscriptionPlanDetails(customerId , subscriptionId);
                }
                else if (stripeEvent.Type == Events.InvoicePaymentActionRequired)
                {
                    var paymentIntent = stripeEvent.Data.Object as Invoice;
                    await _paymentService.SaveWebhookEvent(paymentIntent.CustomerId, null, json, type);
                    Console.WriteLine("Invoice Payment was failed!");
                    _emailService.SendEmail("narsing.m@synsoftglobal.com", "EMS Webhook - Invoice Payment failed", json);
                    _emailService.SendEmail("mahimakhore.synsoft@gmail.com", "EMS Webhook - Invoice Payment failed", json);
                }
                else if (stripeEvent.Type == Events.InvoicePaymentSucceeded)
                {
                    var paymentIntent = stripeEvent.Data.Object as Invoice;
                }
                else if (stripeEvent.Type == Events.PaymentIntentPaymentFailed)
                {
                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    var customerId = paymentIntent?.CustomerId;
                    var customerEmail = paymentIntent?.LastPaymentError?.PaymentMethod?.BillingDetails?.Email;
                    await _paymentService.SaveWebhookEvent(paymentIntent.CustomerId, null, json, type);
                    Console.WriteLine("Payment Intent was failed!");
                    _emailService.SendEmail("narsing.m@synsoftglobal.com", "EMS Webhook - Payment Intent failed", json);
                    _emailService.SendEmail("mahimakhore.synsoft@gmail.com", "EMS Webhook - Payment Intent failed", json);
                }
                else if (stripeEvent.Type == Events.SubscriptionScheduleAborted)
                {
                    var paymentIntent = stripeEvent.Data.Object as SubscriptionSchedule;
                    var customerEmail = paymentIntent?.Customer?.Email;
                    await _paymentService.SaveWebhookEvent(paymentIntent.CustomerId, null, json, type);
                    Console.WriteLine("Subscription schedule aborted");
                    _emailService.SendEmail("narsing.m@synsoftglobal.com", "EMS Webhook - Subscription schedule aborted ", json);
                    _emailService.SendEmail("mahimakhore.synsoft@gmail.com", "EMS Webhook - Subscription schedule aborted ", json);
                }
                else if (stripeEvent.Type == Events.CustomerSubscriptionUpdated)
                {
                    var paymentIntent = stripeEvent.Data.Object as Subscription;

                    await _paymentService.SaveWebhookEvent(paymentIntent.CustomerId, null, json, type);
                    Console.WriteLine("Unhandled event type: {0}", stripeEvent.Type);
                    _emailService.SendEmail("narsing.m@synsoftglobal.com", "EMS Webhook - Subscription schedule aborted ", json);
                    _emailService.SendEmail("mahimakhore.synsoft@gmail.com", "EMS Webhook - Subscription schedule aborted ", json);
                }

                return Ok();
            }
            catch (StripeException ex)
            {
                _emailService.SendEmail("narsing.m@synsoftglobal.com", "EMS Webhook failed", ex.Message);
                _emailService.SendEmail("mahimakhore.synsoft@gmail.com", "EMS Webhook failed", ex.Message);

                return Ok();
            }
        }
    }
}
