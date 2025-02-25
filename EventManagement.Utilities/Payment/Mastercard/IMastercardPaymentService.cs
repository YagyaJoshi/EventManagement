namespace EventManagement.Utilities.Payment.Mastercard
{
    public interface IMastercardPaymentService
    {
        Task<string> CreateSession(string merchantId, string apiPassword, long orderId, decimal amount, string currency, string successUrl, string cancelUrl, string returnUrl, decimal? walletAmount, string organizationName, string email, long organizationId, long userId, decimal penaltyAmount, decimal actualAmount, string paymentUrl, string eventName, int apiVersion);

        Task<string> CreateSessionForPenality(string merchantId, string apiPassword, long orderId, decimal amount, string currency, string successUrl, string cancelUrl, long penalityId, long oldOrderId, long? newOrderId, string? passportFirstName, string? passportLastName, string? passportNumber, string organizationName, long organizationId, decimal actualAmount, string paymentUrl, string eventName, int apiVersion);

        Task<string> CreateSessionForWallet(string merchantId, string apiPassword, long userId, long organizationId, decimal amount, string successUrl, string cancelUrl, string organizationName, string email, decimal actualAmount, string paymentUrl, string currency, int apiVersion);
        Task<OrderDetailsResponse> GetOrderDetails(string orderId, string merchantId, string apiPassword, int apiVersion, string paymentUrl);

        Task<string> RefundPayment(long orderId, decimal amount, string currency);
    }
}
