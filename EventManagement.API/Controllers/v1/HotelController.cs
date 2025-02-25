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
    public class HotelController : BaseController
    {
        private readonly IHotelServices _hotelServices;

        public HotelController(IHotelServices hotelServices, IEmailService emailService): base(emailService)
        {
            _hotelServices = hotelServices;
        }

        [Route("")]
        [HttpPost]
        [Authorize(Roles = "superAdmin,admin")]
        public async Task<IActionResult> AddHotel([FromBody] Hotel input)
        {
            long organizationId = (HttpContext.Items["OrganizationId"] as long?) ?? 0;
            return await ExecuteAsync(() => _hotelServices.AddHotel(null, organizationId, input), Resource.ADD_HOTEL_SUCCESS);
        }

        [Route("{id}")]
        [HttpPut]
        [Authorize(Roles = "superAdmin,admin")]
        public async Task<IActionResult> UpdateHotel([FromRoute] long id, [FromBody] Hotel input)
        {
            long organizationId = (HttpContext.Items["OrganizationId"] as long?) ?? 0;
            return await ExecuteAsync(() => _hotelServices.AddHotel(id, organizationId, input), Resource.UPDATE_HOTEL_SUCCESS);
        }

        [HttpGet]
        [Authorize]
        [Route("all")]
        public async Task<IActionResult> GetAllHotels(string sortColumn = null, string sortOrder = null, string searchText = null, int? pageNo = 1, int? pageSize = 10)
        {
            long organizationId = (HttpContext.Items["OrganizationId"] as long?) ?? 0;
            return await ExecuteAsync(() => _hotelServices.GetAllHotels(organizationId, sortColumn, sortOrder, searchText, pageNo, pageSize), Resource.SUCCESS);
        }

        [Route("{id}")]
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetHotelById([FromRoute] long id)
        {
            return await ExecuteAsync(() => _hotelServices.GetHotelById(id), Resource.SUCCESS);
        }

        [HttpDelete]
        [Authorize(Roles = "superAdmin,admin")]
        [Route("{id}")]
        public async Task<IActionResult> DeleteHotel([FromRoute] long id)
        {
            return await ExecuteAsync(() => _hotelServices.DeleteHotel(id), Resource.DELETE_HOTEL_SUCCESS);
        }

        [Route("getRoomAvailability")]
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CheckRoomAvailability([FromBody]RoomAvailabilityInput input)
        {
            return await ExecuteAsync(() => _hotelServices.CheckRoomAvailability(input), Resource.SUCCESS);
        }

        [Route("details")]
        [HttpGet]
        [Authorize(Roles = "superAdmin,admin,transportationStaff,financeStaff,accommodationStaff,supportStaff")]
        public async Task<IActionResult> GetHotelsByOrganizationId()
        {
            long organizationId = (HttpContext.Items["OrganizationId"] as long?) ?? 0;
            return await ExecuteAsync(() => _hotelServices.GetHotelsByOrganizationId(organizationId), Resource.SUCCESS);
        }
    }
}
