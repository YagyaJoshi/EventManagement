using EventManagement.DataAccess.Models;

namespace EventManagement.DataAccess.ViewModels.Dtos
{
    public class HotelDto
    {
        public long Id { get; set; }
        public long OrganizationId { get; set; }
        public string Name { get; set; }
        public double Rating { get; set; }
        public string Address { get; set; }
        public string PostalCode { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public int? CountryId { get; set; }
        public string Country { get; set; }
        public string LocationLatLong { get; set; } = string.Empty;
        //public string Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public List<int> EventIds { get; set; } = new List<int>();
        public List<HotelRoomType> RoomType { get; set; } = new List<HotelRoomType>();
    }
}
