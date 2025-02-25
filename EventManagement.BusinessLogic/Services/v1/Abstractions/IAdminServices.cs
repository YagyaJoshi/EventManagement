
using EventManagement.DataAccess.ViewModels.ApiObjects;
using EventManagement.DataAccess.ViewModels.Dtos;

namespace EventManagement.BusinessLogic.Services.v1.Abstractions
{
    public interface IAdminServices
    {
        Task<long> AddAccreditationTemplate(long organizationId,AccreditationInput input);

        Task<AccreditationListDto> GetAccreditationTemplate(long? organizationId, int? pageNo, int? pageSize, string? sortOrder);
    }
}
