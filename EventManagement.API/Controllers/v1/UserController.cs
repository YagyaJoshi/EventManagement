using EventManagement.BusinessLogic.Resources;
using EventManagement.BusinessLogic.Services.v1.Abstractions;
using EventManagement.DataAccess.ViewModels.ApiObjects;
using EventManagement.Utilities.Email;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventManagement.API.Controllers.v1
{
    [ApiController]
    [Produces("application/json")]
    [Route("api/v" + EventManagement.BusinessLogic.Version.Value + "/[controller]")]
    public class UserController : BaseController
    {
        private readonly IUserServices _usersServices;


        public UserController(IUserServices usersServices, IEmailService emailService) : base(emailService)
        {
            _usersServices = usersServices;
        }

        [Route("profile/update")]
        [Authorize]
        [HttpPut]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserInput model)
        {
            long userId = (HttpContext.Items["UserId"] as long?) ?? 0;
            var role = HttpContext.Items["UserRole"];

            return await ExecuteAsync(() => _usersServices.UpdateUser(userId, role.ToString(), model), Resource.UPDATE_PROFILE_SUCCESS);
        }

        [HttpGet]
        [Authorize]
        [Route("profile")]
        public async Task<IActionResult> GetUserById()
        {
            long userId = (HttpContext.Items["UserId"] as long?) ?? 0;
            string userRole = HttpContext.Items["UserRole"] as string ?? string.Empty;

            if(userId <= 0 || string.IsNullOrEmpty(userRole))
                throw new UnauthorizedAccessException(Resource.INVALID_TOKEN);

            if (userRole.ToLower() == "admin")
                return await ExecuteAsync(() => _usersServices.GetAdminProfileById(userId), Resource.SUCCESS);

            return await ExecuteAsync(() => _usersServices.GetUserById(userId), Resource.SUCCESS);
        }


        [Route("all")]
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllUsers(int pageNo = 1, int pageSize = 10, string sortColumn = null, string sortOrder = null, string searchText = null)
        {
            long organizationId = (HttpContext.Items["OrganizationId"] as long?) ?? 0;

            return await ExecuteAsync(() => _usersServices.GetAllUsers(organizationId, pageNo, pageSize, sortColumn, sortOrder, searchText), Resource.SUCCESS);
        }


        [HttpGet]
        [Authorize]
        [Route("details/{id}")]
        public async Task<IActionResult> GetCustomerDetailsById([FromRoute]long id)
        {
            long organizationId = (HttpContext.Items["OrganizationId"] as long?) ?? 0;
            return await ExecuteAsync(() => _usersServices.GetCustomerDetailsById(id, organizationId), Resource.SUCCESS);
        }

        [HttpPut]
        [Authorize]
        [Route("fcmToken/update")]
        public async Task<IActionResult> UpdateUserFcmTokenAsync([FromBody] FcmTokenInput input)
        {
            //long organizationId = (HttpContext.Items["OrganizationId"] as long?) ?? 0;
            return await ExecuteAsync(() => _usersServices.UpdateUserFcmTokenAsync(input), Resource.SUCCESS);
        }


    }
}
