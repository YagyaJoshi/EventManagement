
using EventManagement.DataAccess.Enums;

namespace EventManagement.DataAccess.ViewModels.Dtos
{
    public class VisaGuestDto : GuestDetailDto
    {
        public OrdersDto Order { get; set; }
        //public EventBookings Event { get; set; }

        public CustomerDetail Customer { get; set; }
    }

    public class OrdersDto
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public long OrganizationId { get; set; }
        public string Status { get; set; }
        public string PaymentStatus { get; set; }
        public string OrderDate { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal TotalAmount {  get; set; }
        public bool IsAccommodationEnabled { get; set; }

        public bool IsTicketingSystemEnabled { get; set; }

        public bool IsVisaEnabled { get; set; }
    }

    //public class EventBookings
    //{
    //    public long Id { get; set; }
    //    public string Name { get; set; }
    //}

    public class CustomerDetail
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public int? CountryId { get; set; }
        public string Country { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        public string OrganizationType { get; set; }

        public string OrganizationName { get; set;}
    }
}
