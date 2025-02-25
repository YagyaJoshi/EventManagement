using EventManagement.BusinessLogic.Exceptions;
using EventManagement.BusinessLogic.Services.v1.Abstractions;
using EventManagement.DataAccess.Enums;
using EventManagement.DataAccess;
using EventManagement.DataAccess.ViewModels.ApiObjects;
using EventManagement.DataAccess.ViewModels.Dtos;
using EventManagement.Utilities.Payment.Mastercard;
using System.Data.SqlClient;
using System.Data;
using Microsoft.Extensions.Configuration;
using EventManagement.BusinessLogic.Helpers;
using Newtonsoft.Json;
using EventManagement.BusinessLogic.Resources;
using EventManagement.Utilities.Email;
using Microsoft.Extensions.Hosting;
using EventManagement.DataAccess.Models;
using System.Text;

namespace EventManagement.BusinessLogic.Services.v1.Implementations
{
    public class PaymentService : IPaymentService
    {
        public readonly ICustomerServices _customerServices;
        private readonly IConfiguration _configuration;
        public readonly IMastercardPaymentService _mastercardPaymentService;
        public readonly IOrganizationServices _organizationServices;
        public readonly IEmailService _emailService;
        private readonly IHostEnvironment _env;
        private readonly IAuthServices _authServices;

        public PaymentService(ICustomerServices customerServices, IMastercardPaymentService mastercardPaymentService, IConfiguration configuration, IOrganizationServices organizationServices, IEmailService emailService, IHostEnvironment env, IAuthServices authServices)
        {
            _customerServices = customerServices;
            _mastercardPaymentService = mastercardPaymentService;
            _configuration = configuration;
            _organizationServices = organizationServices;
            _emailService = emailService;
            _env = env;
            _authServices = authServices;
        }

        //Not in use
        //public async Task<CreateSessionDto> CreateSession(CreateSessionInput input)
        //{
        //    var bookingDetails = await _customerServices.GetBookingDetailsId(input.OrderId, false
        //        );

        //    if (bookingDetails == null)
        //        throw new ServiceException(Resources.Resource.BOOKING_NOT_FOUND);

        //    var walletAmount = input.UseWalletBalance ? bookingDetails.WalletAmount : 0;

        //    var amount = walletAmount > 0 ? bookingDetails.TotalWalletAmountInDisplayCurrency - bookingDetails.TotalAmountInDisplayCurrency : bookingDetails.TotalAmountInDisplayCurrency;

        //    string sessionId = await _mastercardPaymentService.CreateSession(bookingDetails.MerchantId, bookingDetails.ApiPassword, input.OrderId, Convert.ToDecimal(amount), bookingDetails.CurrencyCode, input.SuccessUrl, input.CancelUrl, $"{_configuration["AppSettings:ApiUrl"]}api/v1/payment/order/OrderId_{input.OrderId}", walletAmount, bookingDetails.OrganizationName, bookingDetails.User.Email, bookingDetails.OrganizationId, bookingDetails.UserId, bookingDetails.PenaltyAmount);

        //    if (string.IsNullOrEmpty(sessionId))
        //        throw new ServiceException(Resource.UnexpectedError);

        //    return new CreateSessionDto
        //    {
        //        SessionId = sessionId,
        //        //PaymentRedirectUrl = $"{_configuration["MasterCardApi:BaseUrl"]}/checkout/pay/{sessionId}?checkoutVersion=1.0.0"
        //        PaymentRedirectUrl = $"{_configuration["MasterCardApi:BaseUrl"]}/checkout/pay/{sessionId}"
        //    };
        //}

        public async Task<long> SavePaymentDetail(TransactionDetailInput input)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_SavePaymentDetails");

            try
            {
                objCmd.Parameters.AddWithValue("@OrderId", input.OrderId);
                objCmd.Parameters.AddWithValue("@OrganizationId", input.OrganizationId);
                objCmd.Parameters.AddWithValue("@TransactionId", input.TransactionId);
                objCmd.Parameters.AddWithValue("@TransactionDetails", input.TransactionDetails);
                objCmd.Parameters.AddWithValue("@PaymentType", input.PaymentType.ToLower());
                objCmd.Parameters.AddWithValue("@Status", Status.Booked);
                if(input.Amount > 0)
                    objCmd.Parameters.AddWithValue("@Amount", input.Amount);
                if(input.WalletAmount > 0)
                    objCmd.Parameters.AddWithValue("@WalletAmount", input.WalletAmount);
                objCmd.Parameters.AddWithValue("@WalletStatus", WalletStatus.creditOut);
                objCmd.Parameters.AddWithValue("@PenaltyAmount", input.PenaltyAmount);


                DataTable dtGuest = await objSQL.FetchDT(objCmd);

                var error = Convert.ToInt64(dtGuest.Rows[0]["ErrorCode"]);
                var errorMessage = CommonUtilities.GetErrorMessage(error);
                if (!string.IsNullOrEmpty(errorMessage))
                    throw new ServiceException(errorMessage);

                return Convert.ToInt64(dtGuest.Rows[0]["Id"]);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (objSQL != null) objSQL.Dispose();
                if (objCmd != null) objCmd.Dispose();
            }
        }


        public async Task<long> UpdatePaymentDetails(TransactionDetailInput input)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_UpdatePaymentDetails");

            try
            {
                objCmd.Parameters.AddWithValue("@OrderId", input.OrderId);
                objCmd.Parameters.AddWithValue("@OrganizationId", input.OrganizationId);
                objCmd.Parameters.AddWithValue("@TransactionId", input.TransactionId);
                objCmd.Parameters.AddWithValue("@TransactionDetails", input.TransactionDetails);
                objCmd.Parameters.AddWithValue("@PaymentType", input.PaymentType.ToLower());
                objCmd.Parameters.AddWithValue("@Status", Status.Booked);
                if (input.Amount > 0)
                    objCmd.Parameters.AddWithValue("@Amount", input.Amount);
                if (input.WalletAmount > 0)
                    objCmd.Parameters.AddWithValue("@WalletAmount", input.WalletAmount);
                objCmd.Parameters.AddWithValue("@WalletStatus", WalletStatus.creditOut);

                DataTable dtGuest = await objSQL.FetchDT(objCmd);

                var error = Convert.ToInt64(dtGuest.Rows[0]["ErrorCode"]);
                var errorMessage = CommonUtilities.GetErrorMessage(error);
                if (!string.IsNullOrEmpty(errorMessage))
                    throw new ServiceException(errorMessage);

                return Convert.ToInt64(dtGuest.Rows[0]["Id"]);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (objSQL != null) objSQL.Dispose();
                if (objCmd != null) objCmd.Dispose();
            }
        }

        public async Task<long> SavePenaltyPaymentDetail(TransactionDetailInput input)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_SaveReplaceGuestPenalty");

            try
            {
                objCmd.Parameters.AddWithValue("@OrderId", input.OrderId);
                objCmd.Parameters.AddWithValue("@TransactionId", input.TransactionId);
                objCmd.Parameters.AddWithValue("@TransactionDetails", input.TransactionDetails);
                objCmd.Parameters.AddWithValue("@PaymentTypeId", PaymentType.card);
                objCmd.Parameters.AddWithValue("@Note", "Guest Replacement");
                objCmd.Parameters.AddWithValue("@Amount", input.Amount);

                DataTable dtGuest = await objSQL.FetchDT(objCmd);

                return Convert.ToInt64(dtGuest.Rows[0]["Id"]);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (objSQL != null) objSQL.Dispose();
                if (objCmd != null) objCmd.Dispose();
            }
        }

        public async Task<OrderDetailsResponse> GetOrderDetails(string orderId, long organizationId)
        {
            var merchantDetails = await _organizationServices.GetOrganizationPaymentProviders(organizationId);
            var details = _mastercardPaymentService.GetOrderDetails(orderId, merchantDetails.MerchantId, merchantDetails.ApiPassword, merchantDetails.ApiVersion, merchantDetails.PaymentUrl).Result;
            var orderDetails = details.transaction.Select(e => e.order.custom).FirstOrDefault();
            if (details != null)
            {
                var transDetail = new TransactionDetailInput()
                {
                    OrderId = Convert.ToInt64(orderId.Split('_')[1]),
                    TransactionId = details.authentication.transactionId,
                    TransactionDetails = JsonConvert.SerializeObject(details),
                    PaymentType = details.sourceOfFunds.type,
                    Amount = Convert.ToDecimal(orderDetails.actualAmount),
                    WalletAmount = Convert.ToDecimal(orderDetails.walletAmount),
                    OrganizationId = Convert.ToInt64(orderDetails.organizationId),
                    PenaltyAmount = Convert.ToDecimal(orderDetails.penaltyAmount),
                };
                await SavePaymentDetail(transDetail);

                _authServices.SendAccountInformation(transDetail.OrderId, transDetail.OrganizationId, Convert.ToInt64(orderDetails.userId));
            }
            return details;
        }

        public async Task<OrderDetailsResponse> PayUnpaidAmount(string orderId, long organizationId)
        {
            var merchantDetails = await _organizationServices.GetOrganizationPaymentProviders(organizationId);
            var details = _mastercardPaymentService.GetOrderDetails(orderId, merchantDetails.MerchantId, merchantDetails.ApiPassword, merchantDetails.ApiVersion, merchantDetails.PaymentUrl).Result;
            var orderDetails = details.transaction.Select(e => e.order.custom).FirstOrDefault();
            if (details != null)
            {
                var transDetail = new TransactionDetailInput()
                {
                    OrderId = Convert.ToInt64(orderId.Split('_')[1]),
                    TransactionId = details.authentication.transactionId,
                    TransactionDetails = JsonConvert.SerializeObject(details),
                    PaymentType = details.sourceOfFunds.type,
                    Amount = Convert.ToDecimal(orderDetails.actualAmount),
                    WalletAmount = Convert.ToDecimal(orderDetails.walletAmount),
                    OrganizationId = Convert.ToInt64(orderDetails.organizationId),
                };
                await UpdatePaymentDetails(transDetail);

                _authServices.UpdatePaymentDetails(transDetail.OrderId, organizationId, Convert.ToInt64(orderDetails.userId));
            }
            return details;
        }


        public async Task<OrderDetailsResponse> GetPenaltyDetails(string orderId, long organizationId)
        {
            var merchantDetails = await _organizationServices.GetOrganizationPaymentProviders(organizationId);
            var details = _mastercardPaymentService.GetOrderDetails(orderId, merchantDetails.MerchantId, merchantDetails.ApiPassword, merchantDetails.ApiVersion, merchantDetails.PaymentUrl).Result;

            if (details != null)
            {
                var actualOrderId = Convert.ToInt64(orderId.Split('_')[0]);
                var orderDetails = details.transaction.Select(e => e.order.custom).FirstOrDefault();
                var transDetail = new TransactionDetailInput()
                {
                    OrderId = actualOrderId,
                    TransactionId = details.transaction.FirstOrDefault().transaction.id,
                    TransactionDetails = JsonConvert.SerializeObject(details),
                    PaymentType = details.sourceOfFunds.type,
                    Amount = Convert.ToDecimal(orderDetails.actualAmount),
                };
                SavePenaltyPaymentDetail(transDetail);

                var input = new ReplaceGuestInput 
                {
                    OrderId = actualOrderId,
                    OldGuestId = orderDetails.oldGuestId,
                    NewGuestId = orderDetails.newGuestId,
                    PassportFirstName = orderDetails.passportFirstName,
                    PassportLastName = orderDetails.passportLastName,
                    PassportNumber = orderDetails.passportNumber
                };  

                var replace = _customerServices.ReplaceGuest(input, organizationId, true);
            }
            return details;
        }

        public async Task<OrderDetailsResponse> GetCustomerWalletDetails(string orderId, long organizationId)
        {
            var merchantDetails = await _organizationServices.GetOrganizationPaymentProviders(organizationId);
            var details = _mastercardPaymentService.GetOrderDetails(orderId, merchantDetails.MerchantId, merchantDetails.ApiPassword, merchantDetails.ApiVersion, merchantDetails.PaymentUrl).Result;

            if (details != null)
            {
                var orderDetails = details.transaction.Select(e => e.order.custom).FirstOrDefault();
                var userId = Convert.ToInt64(orderId.Split('_')[1]);
                organizationId = Convert.ToInt64(orderDetails.organizationId);
                var input = new UpdateWalletInput()
                {
                    Amount = Convert.ToDecimal(orderDetails.actualAmount),
                    
                };
                await _customerServices.UpdateWallet(organizationId, userId, input);
            }
            return details;
        }

        public async Task<CreateSessionDto> CreateSessionForWallet(long organizationId, long userId, CreateSessionForWalletInput input)
        {
            var details = await _customerServices.GetCustomerBasicDetails(userId);

           if (string.IsNullOrEmpty(details.MerchantId) && string.IsNullOrEmpty(details.ApiPassword))
                throw new ServiceException(Resource.PAYMENT_SETUP_MISSING);

           var amountInDefaultCurrency = CurrencyHelper.CalculateDefaultCurrencyAmount(input.Amount, details.DisplayCurrencyRate);

            string sessionId = await _mastercardPaymentService.CreateSessionForWallet(details.MerchantId, details.ApiPassword, organizationId, userId, amountInDefaultCurrency, input.SuccessUrl, input.CancelUrl, details.OrganizationName, details.Email, input.Amount, details.PaymentUrl, details.CurrencyCode, details.ApiVersion);

            if (string.IsNullOrEmpty(sessionId))
                throw new ServiceException(Resource.UnexpectedError);

            return new CreateSessionDto
            {
                SessionId = sessionId,
                PaymentRedirectUrl = $"{details.PaymentUrl}/checkout/pay/{sessionId}?checkoutVersion=1.0.0"
            };

        }

        public async Task<CreateBookingDto> CreateBooking(CreateBookingInput input)
        {
            var bookingDetails = await _customerServices.GetBookingDetailsId(input.OrderId, input.IsPayingUnpaidAmount);

            if (bookingDetails == null)
                throw new ServiceException(Resources.Resource.BOOKING_NOT_FOUND);

            if (string.IsNullOrEmpty(bookingDetails.MerchantId) && string.IsNullOrEmpty(bookingDetails.ApiPassword))
                throw new ServiceException(Resource.PAYMENT_SETUP_MISSING);

            string redirectUrl = string.Empty;
            decimal remainingAmount = bookingDetails.TotalAmountInDisplayCurrency;

            // Calculate wallet amount if used
            var walletAmount = input.UseWalletBalance ? bookingDetails.WalletAmount : 0;

            // Calculate remaining amount after applying wallet amount
            if (walletAmount > 0)
            {
                if (walletAmount > remainingAmount)
                {
                    walletAmount = remainingAmount; // Use only the amount that's left
                }

                remainingAmount -= walletAmount; // Subtract walletAmount from remainingAmount
            }

            var remainingAmountInDefaultCurrency =  CurrencyHelper.CalculateDefaultCurrencyAmount(remainingAmount, bookingDetails.DisplayCurrencyRate);

           var amount = CurrencyHelper.CalculateDefaultCurrencyAmount(remainingAmount - bookingDetails.AmountPaid, bookingDetails.DisplayCurrencyRate);

            if (remainingAmount <= 0)
            {
                // Wallet amount covers total order amount, so just save the payment details
                var transactionInput = new TransactionDetailInput
                {
                    OrderId = input.OrderId,
                    PaymentType = input.PaymentType,
                    Amount = bookingDetails.TotalAmountInDisplayCurrency,
                    WalletAmount = walletAmount,
                    OrganizationId = bookingDetails.OrganizationId,
                    PenaltyAmount = bookingDetails.PenaltyAmount
                };
                await SavePaymentDetail(transactionInput);
                redirectUrl = input.SuccessUrl;

                if(input.IsPayingUnpaidAmount)
                    _authServices.UpdatePaymentDetails(input.OrderId, bookingDetails.OrganizationId, bookingDetails.UserId);
                else
                    _authServices.SendAccountInformation(input.OrderId, bookingDetails.OrganizationId, bookingDetails.UserId);
            }
            else
            {
                // Payment processing based on payment type
                switch (input.PaymentType.ToLower())
                {
                    case "card":
                        // Create session for remaining amount
                        string sessionId = await _mastercardPaymentService.CreateSession(
                            bookingDetails.MerchantId,
                            bookingDetails.ApiPassword,
                            input.OrderId,
                            input.IsPayingUnpaidAmount ? amount : remainingAmountInDefaultCurrency,
                            bookingDetails.CurrencyCode,
                            input.SuccessUrl,
                            input.CancelUrl,
                            input.IsPayingUnpaidAmount ? $"{_configuration["AppSettings:ApiUrl"]}api/v1/payment/unpaid/" : $"{_configuration["AppSettings:ApiUrl"]}api/v1/payment/order/",
                            walletAmount,
                            bookingDetails.OrganizationName,
                            bookingDetails.User.Email,
                            bookingDetails.OrganizationId,
                            bookingDetails.UserId,
                            input.IsPayingUnpaidAmount ? 0 : bookingDetails.PenaltyAmount,
                            input.IsPayingUnpaidAmount ? remainingAmount - bookingDetails.AmountPaid : remainingAmount,
                            bookingDetails.PaymentUrl,
                            bookingDetails.EventName,
                            bookingDetails.ApiVersion
                        );

                        if (string.IsNullOrEmpty(sessionId))
                            throw new ServiceException(Resources.Resource.UnexpectedError);

                        redirectUrl = $"{bookingDetails.PaymentUrl}/checkout/pay/{sessionId}?checkoutVersion=1.0.0";

                        break;

                    case "cash":
                    case "banktransfer":
                    case "wallet":
                        // Save payment details and redirect to success URL
                        var transactionInput = new TransactionDetailInput
                        {
                            OrderId = input.OrderId,
                            PaymentType = input.PaymentType,
                            Amount = remainingAmount,
                            WalletAmount = walletAmount,
                            OrganizationId = bookingDetails.OrganizationId,
                            PenaltyAmount = bookingDetails.PenaltyAmount
                        };
                        await SavePaymentDetail(transactionInput);

                        if (input.IsPayingUnpaidAmount)
                            _authServices.UpdatePaymentDetails(input.OrderId, bookingDetails.OrganizationId, bookingDetails.UserId);
                        else
                            _authServices.SendAccountInformation(input.OrderId, bookingDetails.OrganizationId, bookingDetails.UserId);

                        redirectUrl = input.SuccessUrl;
                        break;

                    default:
                        throw new ServiceException("Unsupported payment type");
                }
            }

            return new CreateBookingDto
            {
                RedirectUrl = redirectUrl
            };
        }

        public async Task<long> UpdatePaymentStatus(StatusUpdateInput input)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_UpdatePaymentStatus");

            try
            {
                objCmd.CommandType = CommandType.StoredProcedure;
                objCmd.Parameters.AddWithValue("@BookingId", input.BookingId);
                objCmd.Parameters.AddWithValue("@Amount", input.Amount);

                DataTable dtPayment = await objSQL.FetchDT(objCmd);

                var error = Convert.ToInt64(dtPayment.Rows[0]["ErrorCode"]);
                var errorMessage = CommonUtilities.GetErrorMessage(error);
                if (!string.IsNullOrEmpty(errorMessage))
                    throw new ServiceException(errorMessage);

                long organizationId = Convert.ToInt64(dtPayment.Rows[0]["OrganizationId"]);
                long userId = Convert.ToInt64(dtPayment.Rows[0]["UserId"]);
                //decimal penaltyAmount = Convert.ToDecimal(dtPayment.Rows[0]["PenaltyAmount"]);

                _authServices.UpdatePaymentDetails(input.BookingId, organizationId, userId);

                return input.BookingId;
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (objSQL != null) objSQL.Dispose();
                if (objCmd != null) objCmd.Dispose();
            }
        }

        public async Task SaveWebhookEvent(string customerId, string destinationId, string eventData, string type)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_SaveWebhookEvent");

            try
            {
                objCmd.CommandType = CommandType.StoredProcedure;
                objCmd.Parameters.AddWithValue("@CustomerId", customerId);
                objCmd.Parameters.AddWithValue("@DestinationId", destinationId);
                objCmd.Parameters.AddWithValue("@EventData", eventData);
                objCmd.Parameters.AddWithValue("@Type", type);

                await objSQL.UpdateDB(objCmd);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (objSQL != null) objSQL.Dispose();
                if (objCmd != null) objCmd.Dispose();
            }
        }

        public async Task UpdateSubscriptionPlan(string customerId, string email)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_UpdateOrganizationSubscriptionPlan");

            try
            {
                objCmd.Parameters.AddWithValue("@CustomerId", customerId);
                DataTable dtPayment = await objSQL.FetchDT(objCmd);

                var isPlanUpdated =  Convert.ToBoolean(dtPayment.Rows[0]["IsPlanUpdated"]);
                if (isPlanUpdated)
                {
                    List<string> cc = new List<string> { "mahimakhore.synsoft@gmail.com", "narsing.m@synsoftglobal.com" };
                   
                    string template = CommonUtilities.GetEmailTemplateText(_env.ContentRootPath + Path.DirectorySeparatorChar.ToString() + "EmailTemplates" + Path.DirectorySeparatorChar.ToString() + "payment-fail.html");

                    if (!string.IsNullOrEmpty(email))
                        _emailService.SendEmail(email, "EventManagement - Payment Failed", template, cc);
                }

            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (objSQL != null) objSQL.Dispose();
                if (objCmd != null) objCmd.Dispose();
            }
        }

        public async Task UpdateSubscriptionPlanDetails(string customerId, string subscriptionId)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_UpdateSubscriptionPlanDetails");

            try
            {
                DateTimeOffset startDate = DateTimeOffset.UtcNow;
                DateTimeOffset endDate = startDate.AddMonths(1);

                objCmd.CommandType = CommandType.StoredProcedure;
                objCmd.Parameters.AddWithValue("@CustomerId", customerId);
                objCmd.Parameters.AddWithValue("@SubscriptionId", subscriptionId);
                objCmd.Parameters.AddWithValue("@StartDate", startDate);
                objCmd.Parameters.AddWithValue("@EndDate", endDate);

                await objSQL.UpdateDB(objCmd);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (objSQL != null) objSQL.Dispose();
                if (objCmd != null) objCmd.Dispose();
            }
        }

       
    }
}
