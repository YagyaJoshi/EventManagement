
namespace EventManagement.DataAccess.Models
{
    public class HotelAssignedToEvent
    {
        public long Id { get; set; }
        public long HotelId { get; set; }
        public long OrganizationEventIds { get; set; }
    }
}
