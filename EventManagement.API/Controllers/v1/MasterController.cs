using EventManagement.BusinessLogic.Resources;
using EventManagement.BusinessLogic.Services.v1.Abstractions;
using EventManagement.BusinessLogic.Services.v1.Implementations;
using EventManagement.Utilities.Email;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace EventManagement.API.Controllers.v1
{
    [ApiController]
    [Produces("application/json")]
    [Route("api/v" + EventManagement.BusinessLogic.Version.Value + "/[controller]")]
    public class MasterController : BaseController
    {
        private readonly IMasterServices _masterServices;
        private readonly ICustomerServices _customerServices;
        private readonly IOrganizationServices _organizationServices;


        public MasterController(IMasterServices masterServices, IEmailService emailService, ICustomerServices customerServices, IOrganizationServices organizationServices) : base(emailService)
        { 
            _masterServices = masterServices;
            _customerServices = customerServices;
            _organizationServices = organizationServices;
        }

        [HttpGet]
        [Route("timezones")]
        public async Task<IActionResult> GetTimeZones()
        {
            return await ExecuteAsync(() => _masterServices.GetTimeZones(), Resource.SUCCESS);
        }

        [HttpGet]
        [Route("accessiblities")]
        public async Task<IActionResult> GetAccessiblities()
        {
            return await ExecuteAsync(() => _masterServices.GetAccessiblities(), Resource.SUCCESS);
        }

        [HttpGet]
        [Route("currencies")]
        public async Task<IActionResult> GetCurrencies()
        {
            return await ExecuteAsync(() => _masterServices.GetCurrencies(), Resource.SUCCESS);
        }

        [HttpGet]
        [Route("countries")]
        public async Task<IActionResult> GetCountries()
        {
            return await ExecuteAsync(() => _masterServices.GetCountries(), Resource.SUCCESS);
        }

        [Route("bookings/all")]
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllBookings(long? eventId, DateTimeOffset? orderDate, string sortColumn = null, string sortOrder = null, string searchText = null, int? pageNo = 1, int? pageSize = 10)
        {
            long organizationId = (HttpContext.Items["OrganizationId"] as long?) ?? 0;
            long userId = (HttpContext.Items["UserId"] as long?) ?? 0;
            string userRole = HttpContext.Items["UserRole"] as string ?? string.Empty;;
            return await ExecuteAsync(() => _customerServices.GetAllBookings(organizationId, userId, eventId, orderDate, userRole, sortColumn, sortOrder, searchText, pageNo, pageSize), Resource.SUCCESS);
        }

        [Route("booking/{bookingId}")]
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetBookingById([FromRoute] long bookingId)
        {
            return await ExecuteAsync(() => _customerServices.GetBookingById(bookingId), Resource.SUCCESS);
        }

        [HttpGet]
        [Route("templates")]
        public async Task<IActionResult> GetIdCardTemplates()
        {
            return await ExecuteAsync(() => _organizationServices.GetIdCardTemplates(), Resource.SUCCESS);
        }

        [HttpGet]
        [Authorize]
        [Route("all/notification")]
        public async Task<IActionResult> GetAllNotification(int? pageNo = 1, int? pageSize = 10, string sortColumn = null, string sortOrder = null)
        {
            long organizationId = (HttpContext.Items["OrganizationId"] as long?) ?? 0;
            long userId = (HttpContext.Items["UserId"] as long?) ?? 0;
            string userRole = HttpContext.Items["UserRole"] as string ?? string.Empty;
            return await ExecuteAsync(() => _masterServices.GetAllNotification(organizationId,
                userId, pageNo, pageSize, sortColumn, sortOrder, userRole), Resource.SUCCESS);
        }
    }
}
