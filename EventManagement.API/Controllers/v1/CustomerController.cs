using EventManagement.BusinessLogic.Exceptions;
using EventManagement.BusinessLogic.Resources;
using EventManagement.BusinessLogic.Services.v1.Abstractions;
using EventManagement.BusinessLogic.Services.v1.Implementations;
using EventManagement.DataAccess.ViewModels.ApiObjects;
using EventManagement.DataAccess.ViewModels.Dtos;
using EventManagement.Utilities.Email;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventManagement.API.Controllers.v1
{
    [ApiController]
    [Produces("application/json")]
    [Route("api/v" + EventManagement.BusinessLogic.Version.Value + "/[controller]")]
    public class CustomerController : BaseController
    {
        private readonly ICustomerServices _customerServices;

        private readonly IPaymentService _paymentService;

        public CustomerController(ICustomerServices customerServices,IEmailService emailService, IPaymentService paymentService) : base(emailService)
        {
            _customerServices = customerServices;
            _paymentService = paymentService;
        }

        [Route("order/guest")]
        [HttpPost]
        [Authorize(Roles = "customer")]
        public async Task<IActionResult> Booking_AddGuest([FromBody] GuestInput input)
        {
            long organizationId = (HttpContext.Items["OrganizationId"] as long?) ?? 0;
            var userId = (HttpContext.Items["UserId"] as long?) ?? 0;
            return await ExecuteAsync(() => _customerServices.AddGuest(organizationId, null, null, userId, input), Resource.ADD_GUEST_SUCCESS);
        }

        [Route("order/{id}/guest")]
        [HttpPost]
        [Authorize(Roles = "customer")]
        public async Task<IActionResult> Booking_AddGuestDetails([FromRoute] long id, [FromBody] GuestInput input)
        {
            long organizationId = (HttpContext.Items["OrganizationId"] as long?) ?? 0;
            var userId = (HttpContext.Items["UserId"] as long?) ?? 0;
            return await ExecuteAsync(() => _customerServices.AddGuest(organizationId, id, null, userId, input), Resource.ADD_GUEST_SUCCESS);
        }

        [Route("order/{id}/guest/{guestId}")]
        [HttpPut]
        [Authorize(Roles = "customer")]
        public async Task<IActionResult> Booking_UpdateGuest([FromRoute] long id, [FromRoute] long? guestId, [FromBody] GuestInput input)
        {
            long organizationId = (HttpContext.Items["OrganizationId"] as long?) ?? 0;
            var userId = (HttpContext.Items["UserId"] as long?) ?? 0;
            return await ExecuteAsync(() => _customerServices.AddGuest(organizationId, id, guestId, userId, input), Resource.UPDATE_GUEST_SUCCESS);
        }

        // Not in use
        [Route("order/{id}/guest/all")]
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllGuest([FromRoute] long id, [FromQuery] string searchText = null, [FromQuery] string status = null)
        {
            return await ExecuteAsync(() => _customerServices.GetAllGuest(id, searchText, status), Resource.SUCCESS);
        }

        [HttpDelete]
        [Authorize(Roles = "customer")]
        [Route("order/guest/{id}")]
        public async Task<IActionResult> DeleteGuestDetails([FromRoute] long id)
        {
            return await ExecuteAsync(() => _customerServices.DeleteGuest(id), Resource.DELETE_GUEST_SUCCESS);
        }

        //Not in Use
        [Route("order/guest/{id}")]
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetGuestById([FromRoute] long id)
        {
            return await ExecuteAsync(() => _customerServices.GetGuestById(id), Resource.SUCCESS);
        }

        [Route("order/accommodation")]
        [HttpPost]
        [Authorize(Roles = "customer")]
        public async Task<IActionResult> Booking_AddAccommodationInfo([FromBody] AccommodationInput input)
        {
            return await ExecuteAsync(() => _customerServices.Booking_AddAccomodationInfo(false, input), Resource.ADD_HOTEL_BOOKING_SUCCESS);

        }

        [Route("order/accommodation")]
        [HttpPut]
        [Authorize(Roles = "customer")]
        public async Task<IActionResult> Booking_UpdateAccommodationInfo([FromBody] AccommodationInput input)
        {
            return await ExecuteAsync(() => _customerServices.Booking_AddAccomodationInfo(true, input), Resource.UPDATE_HOTEL_BOOKING_SUCCESS);
        }

        [HttpDelete]
        [Authorize(Roles = "customer")]
        [Route("order/accommodation")]
        public async Task<IActionResult> DeleteAccommodationInfo([FromBody] DeletesAccomodationRequest input)
        {
            return await ExecuteAsync(() => _customerServices.DeleteAccommodationInfo(input), Resource.DELETE_ACCOMMODATION_SUCCESS);
        }

        [Route("order/visa")]
        [HttpPut]
        [Authorize(Roles = "customer")]
        public async Task<IActionResult> Booking_UpdatVisaInfo( [FromBody] VisaDetailInput input)
        {
            long organizationId = (HttpContext.Items["OrganizationId"] as long?) ?? 0;
            return await ExecuteAsync(() => _customerServices.Booking_AddVisaInfo(organizationId, input), Resource.UPDATE_VISA_SUCCESS);
        }

        [Route("order/details/{eventId}")]
        [HttpGet]
        [Authorize(Roles = "customer")]
        public async Task<IActionResult> GetDraftOrderByEventId([FromRoute] long eventId)
        {
            var userId = (HttpContext.Items["UserId"] as long?) ?? 0;
            return await ExecuteAsync(() => _customerServices.GetDraftOrderByEventId(eventId, userId), Resource.SUCCESS);
        }


        [Route("guest/transportation")]
        [HttpPut]
        public async Task<IActionResult> UpdateTransportationDetails([FromQuery]long userId,[FromBody] TransportationInput input)
        {
            long organizationId = (HttpContext.Items["OrganizationId"] as long?) ?? 0;
            //var userId = (HttpContext.Items["UserId"] as long?) ?? 0;
            return await ExecuteAsync(() => _customerServices.UpdateTransportationDetails(organizationId, userId,input), Resource.UPDATE_VISA_SUCCESS);
        }

        [Route("guest/replace")]
        [HttpPut]
        [Authorize]
        public async Task<IActionResult> ReplaceGuest([FromBody] ReplaceGuestInput input)
        {
            long organizationId = (HttpContext.Items["OrganizationId"] as long?) ?? 0;
            return await ExecuteAsync(() => _customerServices.ReplaceGuest(input, organizationId), Resource.REPLACE_GUEST_SUCCESS);
        }

        [Route("guest/{guestId}/cancel")]
        [HttpPut]
        [Authorize(Roles = "customer")]
        public async Task<IActionResult> CancelGuest([FromRoute] long guestId)
        {
            return await ExecuteAsync(() => _customerServices.CancelGuest(guestId), Resource.CANCEL_GUEST_SUCCESS);
        }

        [Route("cancel/{orderId}")]
        [HttpPut]
        [Authorize(Roles = "customer")]
        public async Task<IActionResult> CancelBooking([FromRoute] long orderId)
        {
            var refundAmount = await _customerServices.CancelBooking(orderId);

            return await ExecuteAsync(
                () => Task.FromResult(refundAmount),  
                string.Format(Resource.CANCEL_BOOKING_SUCCESS, refundAmount, "\n","\n")
            );
        }

        [Route("ticket")]
        [HttpPost]
        [Authorize(Roles = "customer")]
        public async Task<IActionResult> CreateTicket([FromBody] TicketInput input)
        {
            long organizationId = (HttpContext.Items["OrganizationId"] as long?) ?? 0;
            long userId = (HttpContext.Items["UserId"] as long?) ?? 0;
            return await ExecuteAsync(() => _customerServices.AddUpdateTickets(organizationId, userId, input), Resource.CREATE_TICKET);
        }

        [Route("ticket")]
        [HttpPut]
        [Authorize]
        public async Task<IActionResult> UpdateTicket([FromBody] TicketInput input)
        {
            long organizationId = (HttpContext.Items["OrganizationId"] as long?) ?? 0;
            long userId = (HttpContext.Items["UserId"] as long?) ?? 0;
            return await ExecuteAsync(() => _customerServices.AddUpdateTickets(organizationId,userId, input), Resource.UPDATE_TICKET);
        }

        [Route("ticket/reply")]
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ReplyToTicket([FromBody] ReplyTicketInput input)
        {
            long organizationId = (HttpContext.Items["OrganizationId"] as long?) ?? 0;
            long userId = (HttpContext.Items["UserId"] as long?) ?? 0;
            string userRole = HttpContext.Items["UserRole"] as string ?? string.Empty;;
            return await ExecuteAsync(() => _customerServices.ReplyToTicket(organizationId,userId,userRole,input), Resource.REPLY_TICKET_SUCCESS);
        }

        [Route("ticket/assign")]
        [HttpPut]
        [Authorize(Roles = "admin,supportStaff")]
        public async Task<IActionResult> AssignTicket([FromBody] AssignTicketInput input)
        {
            long userId = (HttpContext.Items["UserId"] as long?) ?? 0;
            string userRole = HttpContext.Items["UserRole"] as string ?? string.Empty;;
            return await ExecuteAsync(() => _customerServices.AssignTicket(userId,userRole, input), Resource.ASSIGN_TICKET_SUCCESS);
        }

        [Route("ticket/close/{ticketId}")]
        [HttpPut]
        [Authorize]
        public async Task<IActionResult> CloseTicket([FromRoute] long ticketId)
        {
            return await ExecuteAsync(() => _customerServices.CloseTicket(ticketId), Resource.CLOSE_TICKET_SUCCESS);
        }


        [Route("tickets/messages")]
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetMessageForTickets(long ticketId)
        {
            string userRole = HttpContext.Items["UserRole"] as string ?? string.Empty;;
            long userId = (HttpContext.Items["UserId"] as long?) ?? 0;
            long organizationId = (HttpContext.Items["OrganizationId"] as long?) ?? 0;
            return await ExecuteAsync(() => _customerServices.GetMessageForTickets(ticketId, userId, organizationId, userRole), Resource.SUCCESS);
        }


        [Route("tickets/all")]
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetTicketsByOrganizationId(int? pageNo, int? pageSize, string sortColumn = null, string sortOrder = null, string searchText = null)
        {
            long organizationId = (HttpContext.Items["OrganizationId"] as long?) ?? 0;
            long userId = (HttpContext.Items["UserId"] as long?) ?? 0;
            string userRole = HttpContext.Items["UserRole"] as string ?? string.Empty;;
            return await ExecuteAsync(() => _customerServices.GetTicketsByOrganizationId(organizationId, userId, userRole, pageNo,
            pageSize, sortColumn, sortOrder, searchText), Resource.SUCCESS);
        }


        [Route("order/guests")]
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetGuestWithoutHotel([FromQuery] long orderId, [FromQuery]  long guestId)
        {
            return await ExecuteAsync(() => _customerServices.GetGuestWithoutHotel(orderId, guestId), Resource.SUCCESS);
        }


        [Route("event/guest/all")]
        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetAccreditationList([FromQuery] long?
        eventId, string? status, string? visaStatus,string? searchText, string? sortOrder, int? pageNo, int? pageSize)
        {
            long organizationId = (HttpContext.Items["OrganizationId"] as long?) ?? 0;
            return await ExecuteAsync(() => _customerServices.GetAccreditationList(organizationId, eventId, status, visaStatus, searchText, sortOrder, pageNo, pageSize), Resource.SUCCESS);
        }


        [Route("wallet")]
        [HttpGet]
        [Authorize(Roles = "customer")]
        public async Task<IActionResult> GetWalletDetails()
        {
            long organizationId = (HttpContext.Items["OrganizationId"] as long?) ?? 0;
            long userId = (HttpContext.Items["UserId"] as long?) ?? 0;
            return await ExecuteAsync(() => _customerServices.GetWalletDetails(organizationId, userId), Resource.SUCCESS);
        }


        [Route("wallet/session")]
        [HttpPost]
        [Authorize(Roles = "customer")]
        public async Task<IActionResult> CreateSessionForWallet(CreateSessionForWalletInput input)
        {
            long organizationId = (HttpContext.Items["OrganizationId"] as long?) ?? 0;
            long userId = (HttpContext.Items["UserId"] as long?) ?? 0;
            return await ExecuteAsync(() => _paymentService.CreateSessionForWallet(organizationId, userId, input), Resource.SUCCESS);
        }

        [Route("wallet/summary")]
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> WalletSummary([FromQuery]int? pageNo,int? pageSize,string sortColumn = null,string sortOrder = null)
        {
            long organizationId = (HttpContext.Items["OrganizationId"] as long?) ?? 0;
            long userId = (HttpContext.Items["UserId"] as long?) ?? 0;
            return await ExecuteAsync(() => _customerServices.WalletSummary(organizationId, userId,pageNo, pageSize, sortColumn, sortOrder), Resource.SUCCESS);
        }

        [Route("upload/bankReceipt")]
        [HttpPut]
        [Authorize(Roles = "customer")]
        public async Task<IActionResult> UploadBankReceiptImage(UploadBankReceiptDto model)
        {
            long organizationId = (HttpContext.Items["OrganizationId"] as long?) ?? 0;

            // Execute the service call and return response
            return await ExecuteAsync(() => _customerServices.UploadBankReceiptImage(organizationId, model), Resource.BANK_RECEIPT_UPLOADED);
        }
    }
}
