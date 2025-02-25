using EventManagement.DataAccess.Models;
using EventManagement.DataAccess.ViewModels.ApiObjects;

namespace EventManagement.DataAccess.ViewModels.Dtos
{
    public class BookingDetailDto
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public long OrganizationId { get; set; }

        public string OrganizationName { get; set; }
        public string OrderDate { get; set; }
        public long EventId { get; set; }
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

        public List<OrderPenaltyDto> OrderPenalties { get; set; } = new List<OrderPenaltyDto>();

        public EventBasicDetails Event { get; set; } = new EventBasicDetails();
        public bool IsAccommodationEnabled { get; set; }

        public bool IsVisaEnabled { get; set; }

        public decimal PaidAmount { get; set; }

        public decimal UnpaidAmount { get; set; }

        public decimal Penalties { get; set; }
        public string PaymentType { get; set; }
        public string BankReceiptImage { get; set; }
    }

    public class BookingBasicDetailDto
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public long OrganizationId { get; set; }

        public decimal DisplayCurrencyRate { get; set; }
        public decimal TotalAmountInDisplayCurrency { get; set; }
        public decimal TotalAmountInDefaultCurrency { get; set; }
        public decimal TotalWalletAmountInDisplayCurrency { get; set; }
        public decimal TotalWalletAmountInDefaultCurrency { get; set; }
        public decimal WalletAmount { get; set; }
        public decimal  AmountPaid {  get; set; }
        public decimal TotalAmount { get; set; }
        public string CurrencyCode { get; set; }
        public string OrganizationName { get; set; }
        public string CustomerEmail { get; set; }
        public string MerchantId { get; set; }
        public string ApiPassword { get; set; }

        public UserDetailsDto User { get; set; }
    
        public decimal PenaltyAmount {  get; set; }

        public string PaymentUrl { get; set; }

        public string EventName { get; set; }

        public int ApiVersion { get; set; }
    }
}
