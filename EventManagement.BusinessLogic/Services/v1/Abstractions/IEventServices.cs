using EventManagement.DataAccess.Models;
using EventManagement.DataAccess.ViewModels.ApiObjects;
using EventManagement.DataAccess.ViewModels.Dtos;

namespace EventManagement.BusinessLogic.Services.v1.Abstractions
{
    public interface IEventServices
    {
        Task<long> AddEvent(long? id, long organizationId, EventInput input);

        Task<EventListDto> GetAllEvents(long organizationId,string sortColumn, string sortOrder, string searchText, int? pageNo, int? pageSize);

        Task<EventDetailsDto> GetById(long id, bool bankDetails = false);

        Task<long> DeleteEvent(long id);
        Task<List<Penalties>> GetPenaltyByEventId(long OrganizationEventId);

        Task<EventListDto> GetEventsByOrganizationId(long organizationId, string sortColumn, string sortOrder, string searchText, int? pageNo, int? pageSize, string status = null);

        Task<bool> ContactUs(long organizationId, ContactUs input);

        Task<long> AddAnnouncement(long? id, long organizationId, Announcement input);

        Task<long> DeleteAnnouncement(long id);

        Task<AnnouncementListDto> GetAllAnnouncements(long organizationId, string sortColumn, string sortOrder, int? pageNo, int? pageSize);

        Task<List<EventDto>> GetEventsByOrganizationId(long organizationId);

    }
}
