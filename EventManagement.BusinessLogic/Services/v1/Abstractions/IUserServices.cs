using EventManagement.DataAccess.Models;
using EventManagement.DataAccess.ViewModels.ApiObjects;
using EventManagement.DataAccess.ViewModels.Dtos;

namespace EventManagement.BusinessLogic.Services.v1.Abstractions
{
    public interface IUserServices
    {
        Task<UpdateUserInput> UpdateUser(long userId,string role, UpdateUserInput model);
        Task<UserProfileDto> GetUserById(long userId);
        Task<AdminProfileDto> GetAdminProfileById(long userId);
        Task<UserDetailsDto> GetUserDetailsById(long userId);
        Task<UserListDto> GetAllUsers(long organizationId, int pageNo = 1, int pageSize = 10, string sortColumn = "", string sortOrder = "", string searchText = null);

        Task<UserResponseDto> GetCustomerDetailsById(long customerId, long organizationId);

        Task<string> UpdateUserFcmTokenAsync(FcmTokenInput input);
       
    }
}
