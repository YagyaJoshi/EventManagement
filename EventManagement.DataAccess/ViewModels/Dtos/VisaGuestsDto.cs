namespace EventManagement.DataAccess.ViewModels.Dtos
{
    public class VisaGuestsDto
    {
        public List<VisaGuestDto> List { get; set; } = new List<VisaGuestDto>();
        public int TotalCount { get; set; }
    }

    public class VisaGuestDetailDto
    {
        public long Id { get; set; }
        public long OrderId { get; set; }
        public long EventId { get; set; }
        public string PassportFirstName { get; set; }
        public string PassportLastName { get; set; }
        public string PassportNumber { get; set; }
        public string Role { get; set; }
        public string VisaStatus { get; set; }
        public OrdersDto Order { get; set; }
        public CustomerDetail Customer { get; set; }
    }
}
