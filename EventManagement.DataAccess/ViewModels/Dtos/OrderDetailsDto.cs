using EventManagement.DataAccess.Models;

namespace EventManagement.DataAccess.ViewModels.Dtos
{
    public class OrderDetailsDto
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public long OrganizationId { get; set; }
        public DateTimeOffset? OrderDate { get; set; }
        public long EventId { get; set; }
        public decimal TotalAmountInDefaultCurrency { get; set; }
        public decimal TotalAmountInDisplayCurrency { get; set; }
        public decimal  DisplayCurrencyRate { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal UnpaidAmount { get; set; }
        public string Status { get; set; }
        public string PaymentStatus { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public List<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

        public List<GuestDetailDto> GuestDetails { get; set; } = new List<GuestDetailDto>();

        public List<AccomodationInfoDto> AccommodationInfo { get; set; } = new List<AccomodationInfoDto>();

        public decimal TotalRegistrationFee { get; set; }
        public bool IsAccommodationEnabled { get; set; }
        public bool IsTicketingSystemEnabled { get; set; }
        public bool IsVisaEnabled { get; set; }
        public decimal Penalties { get; set; }
        public List<OrderPenaltyDto> OrderPenalties { get; set; }
    }
}
