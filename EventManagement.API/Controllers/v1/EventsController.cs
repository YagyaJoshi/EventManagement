    using EventManagement.BusinessLogic.Resources;
using EventManagement.BusinessLogic.Services.v1.Abstractions;
using EventManagement.DataAccess.Models;
using EventManagement.DataAccess.ViewModels.ApiObjects;
using EventManagement.Utilities.Email;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventManagement.API.Controllers.v1
{
    [ApiController]
    [Produces("application/json")]
    [Route("api/v" + EventManagement.BusinessLogic.Version.Value + "/[controller]")]
    public class EventsController : BaseController
    {
        private readonly IEventServices _eventServices;

        public EventsController(IEventServices eventServices, IEmailService emailService) : base(emailService)
        {
            _eventServices = eventServices;
        }

        [Route("")]
        [HttpPost]
        [Authorize(Roles = "superAdmin,admin")]
        public async Task<IActionResult> AddEvent([FromBody] EventInput input)
        {
            long organizationId = (HttpContext.Items["OrganizationId"] as long?) ?? 0;
            return await ExecuteAsync(() => _eventServices.AddEvent(null, organizationId, input), Resource.ADD_EVENT_SUCCESS);
        }

        [Route("{id}")]
        [HttpPut]
        [Authorize(Roles = "superAdmin,admin")]
        public async Task<IActionResult> UpdateEvent([FromRoute] long id, [FromBody] EventInput input)
        {
            long organizationId = (HttpContext.Items["OrganizationId"] as long?) ?? 0;
            return await ExecuteAsync(() => _eventServices.AddEvent(id, organizationId, input), Resource.UPDATE_EVENT_SUCCESS);
        }

        [HttpGet]
        [Authorize]
        [Route("all")]
        public async Task<IActionResult> GetAllEvents(string sortColumn = null, string sortOrder = null, string searchText = null, int? pageNo = 1, int? pageSize = 10)
        {
            long organizationId = (HttpContext.Items["OrganizationId"] as long?) ?? 0;
            return await ExecuteAsync(() => _eventServices.GetAllEvents(organizationId, sortColumn,  sortOrder, searchText, pageNo, pageSize), Resource.SUCCESS);
        }

        [Route("{id}")]
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetById([FromRoute] long id, bool bankDetails = false)
        {
            return await ExecuteAsync(() => _eventServices.GetById(id, bankDetails), Resource.SUCCESS);
        }

        [HttpDelete]
        [Authorize(Roles = "superAdmin,admin")]
        [Route("{id}")]
        public async Task<IActionResult> DeleteEvent([FromRoute] long id)
        {
            return await ExecuteAsync(() => _eventServices.DeleteEvent(id), Resource.DELETE_EVENT_SUCCESS);
        }

        [Route("public/all")]
        [HttpGet]
        public async Task<IActionResult> GetEventsByOrganizationId(string sortColumn = null, string sortOrder = null, string searchText = null, int? pageNo = 1, int? pageSize = 10, string status = null)
        {
            long organizationId = (HttpContext.Items["OrganizationId"] as long?) ?? 0;
            return await ExecuteAsync(() => _eventServices.GetEventsByOrganizationId(organizationId, sortColumn, sortOrder, searchText, pageNo, pageSize, status), Resource.SUCCESS);
        }

        [Route("public/{id}")]
        [HttpGet]
        public async Task<IActionResult> GetEventDetailsById([FromRoute] long id)
        {
            return await ExecuteAsync(() => _eventServices.GetById(id), Resource.SUCCESS);
        }


        [Route("contactUs")]
        [HttpPost]
        public async Task<IActionResult> ContactUs([FromBody] ContactUs input)
        {
            long organizationId = (HttpContext.Items["OrganizationId"] as long?) ?? 0;
            return await ExecuteAsync(() => _eventServices.ContactUs(organizationId, input), Resource.CONTACT_US);
        }

        [Route("addAnnouncement")]
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddAnnouncement([FromBody] Announcement input)
        {
            long organizationId = (HttpContext.Items["OrganizationId"] as long?) ?? 0;
            return await ExecuteAsync(() => _eventServices.AddAnnouncement(null, organizationId, input), Resource.ADD_ANNOUNCEMENT_SUCCESS);
        }

        [Route("announcement/{id}")]
        [HttpPut]
        [Authorize]
        public async Task<IActionResult> UpdateAnnouncement([FromRoute] long id, [FromBody] Announcement input)
        {
            long organizationId = (HttpContext.Items["OrganizationId"] as long?) ?? 0;
            return await ExecuteAsync(() => _eventServices.AddAnnouncement(id, organizationId, input), Resource.UPDATE_ANNOUNCEMENT_SUCCESS);
        }

        [HttpDelete]
        [Authorize]
        [Route("delete/Announcement/{id}")]
        public async Task<IActionResult> DeleteAnnouncement([FromRoute] long id)
        {
            return await ExecuteAsync(() => _eventServices.DeleteAnnouncement(id), Resource.DELETE_ANNOUNCEMENT_SUCCESS);
        }

        [HttpGet]
        [Authorize]
        [Route("all/Announcements")]
        public async Task<IActionResult> GetAllAnnouncements(string sortColumn = null, string sortOrder = null, int? pageNo = 1, int? pageSize = 10)
        {
            long organizationId = (HttpContext.Items["OrganizationId"] as long?) ?? 0;
            return await ExecuteAsync(() => _eventServices.GetAllAnnouncements(organizationId, sortColumn, sortOrder, pageNo, pageSize), Resource.SUCCESS);
        }

        [Route("details")]
        [HttpGet]
        [Authorize(Roles = "superAdmin,admin,transportationStaff,financeStaff,accommodationStaff,supportStaff")]
        public async Task<IActionResult> GetEventsByOrganizationId()
        {
            long organizationId = (HttpContext.Items["OrganizationId"] as long?) ?? 0;
            return await ExecuteAsync(() => _eventServices.GetEventsByOrganizationId(organizationId), Resource.SUCCESS);
        }
    }
}
