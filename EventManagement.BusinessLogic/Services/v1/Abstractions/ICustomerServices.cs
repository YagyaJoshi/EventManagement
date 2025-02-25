using EventManagement.DataAccess.Models;
using EventManagement.DataAccess.ViewModels.ApiObjects;
using EventManagement.DataAccess.ViewModels.Dtos;

namespace EventManagement.BusinessLogic.Services.v1.Abstractions
{
    public interface ICustomerServices
    {
        Task<long> AddGuest(long organizationId, long? orderId, long? guestId, long UserId, GuestInput input);
        Task<List<GuestDetailDto>> GetAllGuest(long? orderId, string searchText = null, string status = null);

        Task<GuestDetailDto> GetGuestById(long guestId);
        Task<long> DeleteGuest(long guestId);

        Task<long> Booking_AddAccomodationInfo(bool isUpdate, AccommodationInput input);

        Task<int> DeleteAccommodationInfo(DeletesAccomodationRequest input);

        Task<long> Booking_AddVisaInfo(long organizationId, VisaDetailInput input);

        Task<OrderDetailsDto> GetDraftOrderByEventId(long eventId, long userId);

        Task<List<OrderDetail>> GetOrderDetailsByOrderId(long orderid);

        Task<BookingListsDto> GetAllBookings(long? organizationId, long? userId, long? eventId, DateTimeOffset? orderDate, string role, string sortColumn, string sortOrder, string searchText, int? pageNo, int? pageSize);

        Task<List<AccomodationInfoDto>> Booking_GetAllAccomodationInfo(long orderId);
        Task<AccomodationInfoDto> Booking_GetAccomodationInfo(long orderId, string guestIds);

        Task<BookingDetailDto> GetBookingById(long bookingId);

        Task<BookingBasicDetailDto> GetBookingDetailsId(long bookingId, bool isPayingUnpaidAmount);

        Task<long> UpdateTransportationDetails(long organizationId, long userId, TransportationInput input);

        Task<ReplaceGuestDto> ReplaceGuest(ReplaceGuestInput input, long organizationId, bool isPenaltyAmountPaid = false);
        Task<long> CancelGuest(long guestId);

        Task<decimal> CancelBooking(long orderId);
        Task<TransportationListDto> GetAllTransportationDetails(long organizationId, long? hotelId, long? eventId, string? date, int? pageNo, int? pageSize, string sortColumn = "", string sortOrder = "", string searchText = "", bool isArrival = true);

        Task<VisaGuestsDto> GetVisaRequiredGuests(long organizationId, long? eventId, string? status, int? pageNo, int? pageSize, string? searchText, string sortOrder, string sortColumn);

        Task<decimal> GetRegistrationFee(long eventId, string role, long? guestId = null);

        Task<long> UpdateVisaStatus(long id, VisaStatusInput input);

        Task<long> AddUpdateTickets(long organizationId, long userId, TicketInput input);

        Task<Ticket> ReplyToTicket(long organizationId, long userId, string userRole, ReplyTicketInput input);

        Task<long> AssignTicket(long userId, string userRole, AssignTicketInput input);

        Task<long> CloseTicket(long ticketId);

        Task<List<Ticket>> GetMessageForTickets(long ticketId, long userId, long organizationId, string userRole);

        Task<TicketDto> GetTicketsByOrganizationId(long organizationId, long? userId, string role, int? pageNo, int? pageSize, string? searchText, string sortColumn, string sortOrder);

        Task<List<GuestDto>> GetGuestWithoutHotel(long orderId, long guestId);
        Task<EventGuestsDto> GetAccreditationList(long organizationId,long? eventId, string? status, string? visaStatus, string? searchText, string? sortOrder, int? pageNo, int? pageSize);
        Task<OrderDto> GetAmountandCurrency(long OrderId);

        Task UpdateWallet(long organizationId, long userId, UpdateWalletInput input);

        Task<WalletDetailDto> GetWalletDetails(long oraganizationId, long userId);

        Task<CustomerBasicDetailDto> GetCustomerBasicDetails(long userId);

        Task<WalletSummaryList> WalletSummary(long organizationId, long userId, int? pageNo, int? pageSize, string sortColumn = null, string sortOrder = null);

        Task<long> UploadBankReceiptImage(long organizationId, UploadBankReceiptDto model);
    }
}
