
using EventManagement.DataAccess.ViewModels.Dtos;
using System.ComponentModel.DataAnnotations;

namespace EventManagement.DataAccess.Models
{
    public class Hotel
    {
        public long? Id { get; set; }
        [Required]
        //[RegularExpression(@"^[a-zA-Z]+(\s[a-zA-Z]+)*$", ErrorMessage = "HotelName cannot contain spaces or special characters.")]
        public string Name { get; set; }
        [Required]
        public double Rating { get; set; }
        [Required]
        public string Address { get; set; }
        [Required]
        public string PostalCode { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string State { get; set; }
        [Required]
        public int CountryId { get; set; }
        public string LocationLatLong { get; set; } = string.Empty;
        //public string Status { get; set; }
        public List<int> EventIds { get; set; } = new List<int>();
        public List<HotelRoomDto> RoomType { get; set; } = new List<HotelRoomDto>();
    }
}
