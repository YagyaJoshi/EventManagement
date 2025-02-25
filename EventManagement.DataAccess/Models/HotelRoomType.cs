
namespace EventManagement.DataAccess.Models
{
    public class HotelRoomType
    {
        public long Id { get; set; }
        public long HotelId { get; set; }
        public string RoomSize { get; set; }
        public decimal PackagePrice { get; set; }
        public int CurrencyId { get; set; }
        public decimal NightPrice { get; set; }
        public int Availability { get; set; }
        public string Status { get; set; }
        public int? MinimumOccupancy { get; set; }
        public int? MaximumOccupancy { get; set;}
    }
}
