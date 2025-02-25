using EventManagement.DataAccess.ViewModels.Dtos;

namespace EventManagement.DataAccess.Models
{
    public class HotelDetailDto
    {
        public long? Id { get; set; }
        public long OrganizationId { get; set; }
        public string Name { get; set; }
        public double Rating { get; set; }
        public string Address { get; set; }
        public string PostalCode { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public int CountryId { get; set; }
        public string LocationLatLong { get; set; } = string.Empty;
        public HotelRoomDto RoomType { get; set; } = new HotelRoomDto();
    }
}
