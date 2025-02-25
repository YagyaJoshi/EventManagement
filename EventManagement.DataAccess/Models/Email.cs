

using EventManagement.DataAccess.ViewModels.Dtos;

namespace EventManagement.DataAccess.Models
{
    public class Email
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public long OrganizationId { get; set; }
        public string OrganizationName { get; set; }
        public string OrderDate { get; set; }
        public long? EventId { get; set; }
        public decimal DisplayCurrencyRate { get; set; }
        public decimal TotalAmountInDefaultCurrency { get; set; }
        public decimal TotalAmountInDisplayCurrency { get; set; }
        public string Status { get; set; }
        public string PaymentStatus { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string CurrencyCode { get; set; }
        public List<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
        public List<GuestDetailDto> GuestDetails { get; set; } = new List<GuestDetailDto>();
        public UserDetailsDto User { get; set; }
        public List<AccomodationInfoDto> AccommodationInfo { get; set; } = new List<AccomodationInfoDto>();

        public bool IsAccommodationEnabled { get; set; }

        public bool IsVisaEnabled { get; set; }

        public decimal PaidAmount { get; set; }

        public decimal UnpaidAmount { get; set; }

        public string OrganizationLogo { get; set; }
        public string EventName { get; set; }

        public decimal VisaFees { get; set; }

        public decimal PenaltyAmount {  get; set; }
    }
}
