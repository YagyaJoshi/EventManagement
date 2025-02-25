using EventManagement.DataAccess.ViewModels.ApiObjects;
using EventManagement.DataAccess.ViewModels.Dtos;
using EventManagement.Utilities.Payment.Mastercard;
using System.Threading.Tasks;

namespace EventManagement.BusinessLogic.Services.v1.Abstractions
{
    public interface IPaymentService
    {

        //Task<CreateSessionDto> CreateSession(CreateSessionInput input);
        Task<long> SavePaymentDetail(TransactionDetailInput input);

        Task<OrderDetailsResponse> GetOrderDetails(string orderId, long organizationId);

        Task<OrderDetailsResponse> GetPenaltyDetails(string orderId, long organizationId);

        Task<OrderDetailsResponse> GetCustomerWalletDetails(string orderId, long organizationId);

        Task<CreateSessionDto> CreateSessionForWallet(long organizationId, long userId, CreateSessionForWalletInput input);

        Task<CreateBookingDto> CreateBooking(CreateBookingInput input);

        Task<long> UpdatePaymentStatus(StatusUpdateInput input);

        Task SaveWebhookEvent(string customerId, string destinationId, string eventData, string type);

        Task UpdateSubscriptionPlan(string customerId, string email);
        Task UpdateSubscriptionPlanDetails(string customerId, string subscriptionId);

        Task<OrderDetailsResponse> PayUnpaidAmount(string orderId, long organizationId);

        Task<long> UpdatePaymentDetails(TransactionDetailInput input);
       
    }
}
