using EventManagement.DataAccess.Models;
using EventManagement.DataAccess.ViewModels.ApiObjects;

namespace EventManagement.BusinessLogic.Services.v1.Abstractions
{
    public interface ISubscriptionServices
    {
        Task<long> CreateSubscription(long organizationId, CreateSubscriptionInput input);

        Task<long> UpdateSubscriptionplan(long? id, UpdateSubscriptionPlanInput input);
        Task<List<SubscriptionPlanMst>> GetSubscriptions();

        Task<SubscriptionPlanMst> GetSubscriptionById(long id);

        Task<bool> CancelSubscription(long organizationId, CancelSubscriptionInput input);
    }
}
