using EventManagement.DataAccess.Models;
using EventManagement.DataAccess.ViewModels.ApiObjects;
using EventManagement.DataAccess.ViewModels.Dtos;

namespace EventManagement.BusinessLogic.Services.v1.Abstractions
{
    public interface IStaffServices
    {
        Task<long> AddorUpdateStaff(long? id, long organizationId, AddStaffInput input);

        Task<StaffListDto> GetAllStaffs(long organizationId, string sortColumn, string sortOrder, string searchText, int? pageNo, int? pageSize);

        Task<StaffDetailsDto> GetStaffById(long id);

        Task<long> DeleteStaff(long id);

        Task<string> UpdateVisaDocument(long bookingId, long guestId, string imgURL);

        Task<List<Roles>> GetStaffRoles();

        Task<List<StaffInfoDto>> GetAssignStaffs(long organizationId);
    }
}
