namespace EventManagement.DataAccess.ViewModels.ApiObjects
{
    public class RoomAvailabilityInput
    {
        public long EventId { get; set; }
        public long HotelId { get; set; }
        public long RoomTypeId { get; set; }
    }
}
