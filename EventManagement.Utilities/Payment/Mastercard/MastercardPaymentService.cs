using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;

namespace EventManagement.Utilities.Payment.Mastercard
{
    public class MastercardPaymentService : IMastercardPaymentService
    {
        private readonly IConfiguration _configuration;
        private static readonly HttpClient client = new HttpClient();
        public MastercardPaymentService(IConfiguration configuration) 
        {
            _configuration = configuration;
        }

     
        public async Task<string> CreateSession(string merchantId, string apiPassword, long orderId, decimal amount, string currency, string successUrl, string cancelUrl, string returnUrl, decimal? walletAmount, string organizationName, string email, long organizationId, long userId, decimal penaltyAmount, decimal actualAmount, string paymentUrl, string eventName, int apiVersion)
        {
            try
            {
                var randomNumber = GenerateRandomString(5);
                var newOrderId =  $"OrderId_{orderId}_{randomNumber}";
                string apiUrl = $"{paymentUrl}/api/rest/version/{apiVersion}/merchant/{merchantId}/session";
                string username = $"merchant.{merchantId}";
                var password = apiPassword;
                var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);

                var requestBody = new
                {
                    apiOperation = "INITIATE_CHECKOUT",
                    checkoutMode = "WEBSITE",
                    interaction = new
                    {
                        operation = "PURCHASE",
                        merchant = new
                        {
                            name = organizationName,
                            url = "https://ems.ilumis.com"
                        },
                        returnUrl = $"{returnUrl}{newOrderId}?organizationId={organizationId}&successUrl={Uri.EscapeDataString(successUrl)}&cancelUrl={Uri.EscapeDataString(cancelUrl)}",
                        cancelUrl = cancelUrl,
                        displayControl = new
                        {
                            billingAddress = "HIDE"
                        }
                    },
                    order = new
                    {
                        currency = currency,
                        amount = amount,
                        id = newOrderId,
                        description = eventName,
                        custom = new
                        {
                            walletAmount = walletAmount.ToString(),
                            organizationId = organizationId.ToString(),
                            userId = userId.ToString(),
                            penaltyAmount =  penaltyAmount.ToString(),
                            actualAmount = actualAmount.ToString()
                        },
                        reference = newOrderId
                    },
                    shipping = new
                    {
                        contact = new
                        {
                            email = email
                        }
                    }
                };

                var json = Newtonsoft.Json.JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(apiUrl, content);
                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    var res = JsonConvert.DeserializeObject<CreateSessionResponse>(responseContent);
                    return res.session.id;
                }
                return null;
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
            
        }

        public async Task<string> CreateSessionForPenality(string merchantId, string apiPassword, long orderId, decimal amount, string currency, string successUrl, string cancelUrl, long penalityId, long oldGuestId, long? newGuestId, string? passportFirstName, string? passportLastName, string? passportNumber, string organizationName, long organizationId, decimal actualAmount, string paymentUrl, string eventName , int apiVersion)
        {
            try
            {
                var randomNumber = GenerateRandomString(5);
                var newOrderId = $"{orderId}_Penalty_{penalityId}_{randomNumber}";
                string apiUrl = $"{paymentUrl}/api/rest/version/{apiVersion}/merchant/{merchantId}/session";
                string username = $"merchant.{merchantId}";
                var password = apiPassword;
                var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);

                var requestBody = new
                {
                    apiOperation = "INITIATE_CHECKOUT",
                    checkoutMode = "WEBSITE",
                    interaction = new
                    {
                        operation = "PURCHASE",
                        merchant = new
                        {
                            name = organizationName,
                            url = "https://ems.ilumis.com"
                        },
                        returnUrl = $"{_configuration["AppSettings:ApiUrl"]}api/v1/payment/order/{newOrderId}/penalty?organizationId={organizationId}&successUrl={Uri.EscapeDataString(successUrl)}&cancelUrl={Uri.EscapeDataString(cancelUrl)}",
                        cancelUrl = cancelUrl.ToString(),
                        displayControl = new
                        {
                            billingAddress = "HIDE"
                        }
                    },
                    order = new
                    {
                        currency = currency,
                        amount = amount.ToString(),
                        id = newOrderId,
                        description = eventName,
                        reference = newOrderId,
                        custom = new
                        {
                            oldGuestId = oldGuestId.ToString(),
                            newGuestId = newGuestId?.ToString(),
                            passportFirstName = string.IsNullOrEmpty(passportFirstName) ? null : passportFirstName.ToString(),
                            passportLastName = string.IsNullOrEmpty(passportLastName) ? null: passportLastName.ToString(),
                            passportNumber = string.IsNullOrEmpty(passportNumber) ? null : passportNumber.ToString(),
                            actualAmount = actualAmount.ToString(),
                        }
                    }
                };

                var json = Newtonsoft.Json.JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(apiUrl, content);
                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    var res = JsonConvert.DeserializeObject<CreateSessionResponse>(responseContent);
                    return res.session.id;
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public static string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            using (var crypto = new RNGCryptoServiceProvider())
            {
                byte[] data = new byte[length];
                crypto.GetBytes(data);

                return new string(data.Select(b => chars[b % chars.Length]).ToArray());
            }
        }

        public async Task<OrderDetailsResponse> GetOrderDetails(string orderId, string merchantId, string apiPassword, int apiVersion, string paymentUrl)
        {
            try
            {
                string apiUrl = $"{paymentUrl}/api/rest/version/{apiVersion}/merchant/{merchantId}/order/{orderId}";
                string username = $"merchant.{merchantId}";
                var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{apiPassword}"));

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);

                var response = await client.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                   
                     var orderDetails = JsonConvert.DeserializeObject<OrderDetailsResponse>(responseContent);
                     return orderDetails;
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<string> RefundPayment(long orderId, decimal amount, string currency)
        {
            try
            {
                var transactionId = $"trans_{GenerateRandomString(6)}";
                // Retrieve necessary configuration values
                var merchantId = _configuration["MasterCardApi:MerchantId"];
                string refundUrl = $"{_configuration["MasterCardApi:BaseUrl"]}/api/rest/version/{_configuration["MasterCardApi:Version"]}/merchant/{merchantId}/order/{orderId}/transaction/{transactionId}";
                string username = $"merchant.{merchantId}";
                var password = _configuration["MasterCardApi:ApiPassword"];
                var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));

                // Set up HTTP client with authorization header
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);

                    // Create the refund request payload
                    var refundRequest = new
                    {
                        transaction = new
                        {
                            amount = amount,
                            currency = currency
                        }
                    };

                    var content = new StringContent(JsonConvert.SerializeObject(refundRequest), Encoding.UTF8, "application/json");

                    // Make the API request for refund
                    var response = await client.PutAsync(refundUrl, content);
                    if (response.IsSuccessStatusCode)
                    {
                        // Parse the response content to get the RefundId
                        string responseContent = await response.Content.ReadAsStringAsync();
                        dynamic responseObject = JsonConvert.DeserializeObject(responseContent);
                        string refundId = responseObject?.transaction?.id;

                        return refundId;
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<string> CreateSessionForWallet(string merchantId, string apiPassword, long organizationId, long userId, decimal amount, string successUrl, string cancelUrl, string organizationName, string email, decimal actualAmount, string paymentUrl, string currency, int apiVersion)
        {
            try
            {
                var randomNumber = GenerateRandomString(5);
                var newOrderId = $"wallet_{userId}_{randomNumber}";               
                string apiUrl = $"{paymentUrl}/api/rest/version/{apiVersion}/merchant/{merchantId}/session";
                string username = $"merchant.{merchantId}";
                var password = apiPassword;
                var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);

                var requestBody = new
                {
                    apiOperation = "INITIATE_CHECKOUT",
                    checkoutMode = "WEBSITE",
                    interaction = new
                    {
                        operation = "PURCHASE",
                        merchant = new
                        {
                            name = organizationName,
                            url = "https://ems.ilumis.com"
                        },
                        returnUrl = $"{_configuration["AppSettings:ApiUrl"]}api/v1/Payment/customer/wallet/{newOrderId}?organizationId={organizationId}&successUrl={Uri.EscapeDataString(successUrl)}&cancelUrl={Uri.EscapeDataString(cancelUrl)}",
                        cancelUrl = cancelUrl.ToString(),
                        displayControl = new
                        {
                            billingAddress = "HIDE"
                        }
                    },
                    order = new
                    {
                        currency = currency,
                        amount = amount.ToString(),
                        id = newOrderId,
                        description = "Wallet Top-Up",
                        custom = new
                        {
                            organizationId = organizationId.ToString(),
                            actualAmount = actualAmount.ToString(),
                        },
                        reference = newOrderId
                    },
                    shipping = new
                    {
                        contact = new
                        {
                            email = email
                        }
                    }
                };

                var json = Newtonsoft.Json.JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(apiUrl, content);
                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    var res = JsonConvert.DeserializeObject<CreateSessionResponse>(responseContent);
                    return res.session.id;
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
