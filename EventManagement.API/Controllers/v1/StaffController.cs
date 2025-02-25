using EventManagement.BusinessLogic.Resources;
using EventManagement.BusinessLogic.Services.v1.Abstractions;
using EventManagement.DataAccess.ViewModels.ApiObjects;
using EventManagement.Utilities.Email;
using EventManagement.Utilities.Storage.AlibabaCloud;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventManagement.API.Controllers.v1
{
    [ApiController]
    [Produces("application/json")]
    [Route("api/v" + EventManagement.BusinessLogic.Version.Value + "/[controller]")]
    public class StaffController : BaseController
    {
        private readonly IStaffServices _staffServices;

        private readonly ICustomerServices _customerServices;

        private readonly IStorageServices _storageServices;
        public StaffController(IStaffServices staffServices, IEmailService emailService, ICustomerServices customerServices, IStorageServices storageServices) : base(emailService)
        {
            _staffServices = staffServices;
            _customerServices = customerServices;
            _storageServices = storageServices;
        }

        [HttpPost]
        [Authorize(Roles = "superAdmin,admin")]
        [Route("")]
        public async Task<IActionResult> AddStaff([FromBody] AddStaffInput input)
        {
            long organizationId = (HttpContext.Items["OrganizationId"] as long?) ?? 0;
            return await ExecuteAsync(() => _staffServices.AddorUpdateStaff(null, organizationId, input), Resource.ADD_STAFF_SUCCESS);
        }

        [HttpPut]
        [Authorize(Roles = "superAdmin,admin")]
        [Route("{id}")]
        public async Task<IActionResult> UpdateStaff([FromRoute] long id, [FromBody] AddStaffInput input)
        {
            long organizationId = (HttpContext.Items["OrganizationId"] as long?) ?? 0;
            return await ExecuteAsync(() => _staffServices.AddorUpdateStaff(id, organizationId, input), Resource.UPDATE_STAFF_SUCCESS);
        }

        [HttpGet]
        [Authorize]
        [Route("all")]
        public async Task<IActionResult> GetAllStaffs(int? pageNo, int? pageSize, string sortColumn = null, string sortOrder = null, string searchText = null)
        {
            long organizationId = (HttpContext.Items["OrganizationId"] as long?) ?? 0;
            return await ExecuteAsync(() => _staffServices.GetAllStaffs(organizationId, sortColumn, sortOrder, searchText, pageNo, pageSize), Resource.SUCCESS);
        }

        [HttpGet]
        [Authorize(Roles = "superAdmin,admin")]
        [Route("{id}")]
        public async Task<IActionResult> GetStaffById([FromRoute] long id)
        {
            return await ExecuteAsync(() => _staffServices.GetStaffById(id), Resource.SUCCESS);
        }

        [HttpDelete]
        [Authorize(Roles = "superAdmin,admin")]
        [Route("{id}")]
        public async Task<IActionResult> DeleteStaff([FromRoute] long id)
        {
            return await ExecuteAsync(() => _staffServices.DeleteStaff(id), Resource.DELETE_STAFF_SUCCESS);
        }

        [Route("booking/{bookingId}")]
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetBookingById([FromRoute] long bookingId)
        {
            return await ExecuteAsync(() => _customerServices.GetBookingById(bookingId), Resource.SUCCESS);
        }

        [Route("guest/transportation/all")]
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetListTransportationDetails(long? hotelId, long? eventId, string? date, int? pageNo, int? pageSize, string sortColumn = "", string sortOrder = "", string searchText = null, bool isArrival = true)
        {
            long organizationId = (HttpContext.Items["OrganizationId"] as long?) ?? 0;
            return await ExecuteAsync(() => _customerServices.GetAllTransportationDetails(organizationId, hotelId, eventId, date, pageNo, pageSize, sortColumn, sortOrder, searchText, isArrival), Resource.SUCCESS);
        }


        [Route("roles")]
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetStaffRoles()
        {
            return await ExecuteAsync(() => _staffServices.GetStaffRoles(), Resource.SUCCESS);
        }


        [Route("guest/all")]
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetVisaRequiredGuests(long? eventId,string? status, int? pageNo, int? pageSize, string? searchText = null, string? sortOrder = null, string? sortColumn = null)
        {
            var organizationId = (long)HttpContext.Items["OrganizationId"];
            return await ExecuteAsync(() => _customerServices.GetVisaRequiredGuests(organizationId, eventId,status, pageNo, pageSize, searchText, sortColumn, sortOrder), Resource.SUCCESS);
        }

        [Route("guest/{id}/visaStatus")]
        [HttpPut]
        [Authorize]
        public async Task<IActionResult> UpdateVisaSatus([FromRoute] long id, VisaStatusInput input)
        {
            return await ExecuteAsync(() => _customerServices.UpdateVisaStatus(id, input), Resource.UPDATE_VISA_STATUS);
        }


        [HttpGet]
        [Authorize]
        [Route("list/assign")]
        public async Task<IActionResult> GetAssignStaffs()
        {
            long organizationId = (HttpContext.Items["OrganizationId"] as long?) ?? 0;
            return await ExecuteAsync(() => _staffServices.GetAssignStaffs(organizationId),Resource.SUCCESS);
        }
    }
}
