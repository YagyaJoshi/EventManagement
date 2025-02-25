using EventManagement.DataAccess.Models;
using EventManagement.DataAccess.ViewModels.Dtos;

namespace EventManagement.BusinessLogic.Services.v1.Abstractions
{
    public interface IMasterServices
    {
        Task<List<TimeZoneDto>> GetTimeZones();

        Task<List<AccessiblityInfoDto>> GetAccessiblities();

        Task<List<CurrencyDto>> GetCurrencies();

        Task<List<object>> GetRoleWiseModules(int roleId);

        Task<List<CountriesMst>> GetCountries();

        Task<NotificationListDto> GetAllNotification(long organizationId, long userId, int? pageNo, int? pageSize, string sortColumn, string sortOrder, string userRole);
    }
}
