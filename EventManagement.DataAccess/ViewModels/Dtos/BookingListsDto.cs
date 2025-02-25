using EventManagement.DataAccess.Models;

namespace EventManagement.DataAccess.ViewModels.Dtos
{
    public class BookingListsDto
    {
        public List<BookingsDto> List { get; set; } = new List<BookingsDto>();
        public int TotalCount { get; set; }
        public int TotalRecords { get; set; }
    }

    public class BookingsDto
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public long OrganizationId { get; set; }
        public string OrderDate { get; set; }
        public long EventId { get; set; }
        public decimal TotalAmountInDefaultCurrency { get; set; }
        public decimal TotalAmountInDisplayCurrency { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal UnpaidAmount { get; set; }
        public string Status { get; set; }
        public string PaymentStatus { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public decimal TotalRegistrationFee { get; set; }
        public UserDetailsDto User { get; set; }
        public EventInfoDto Event { get; set; }
    }
}
