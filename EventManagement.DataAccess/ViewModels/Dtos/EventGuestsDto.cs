namespace EventManagement.DataAccess.ViewModels.Dtos
{
    public class EventGuestsDto
    {
        public List<EventGuestDto> List { get; set; } = new List<EventGuestDto>();
        public int TotalCount { get; set; }
    }
}
