
using EventManagement.DataAccess.ViewModels.Dtos;

namespace EventManagement.DataAccess.Models
{
    public class Orders
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public long OrganizationId { get; set; }
        public string OrderDate { get; set; }
        public long EventId { get; set; }
        public decimal TotalAmountInDefaultCurrency { get; set; }
        public decimal TotalAmountInDisplayCurrency { get; set; }
        public decimal AmountPaid { get; set; }
        public string Status { get; set; }
        public string PaymentStatus { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public List<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
        public List<GuestDetailDto> GuestDetails { get; set; } = new List<GuestDetailDto>();
        public UserDetailsDto User { get; set; }
        public EventDetailsDto Event { get; set; }
        public List<AccomodationInfoDto> AccommodationInfo { get; set; } = new List<AccomodationInfoDto>();
        public decimal TotalRegistrationFee { get; set; }
    }
}
