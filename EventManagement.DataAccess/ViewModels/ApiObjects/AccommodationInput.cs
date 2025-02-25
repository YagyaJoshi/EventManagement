
using System.ComponentModel.DataAnnotations;

namespace EventManagement.DataAccess.ViewModels.ApiObjects
{
    public class AccommodationInput
    {
        public long OrderId { get; set; }

        public long? OrderDetailId { get; set; }

        public long HotelId { get; set; }

        [Required]
        public long HotelRoomTypeId { get; set; }

        [Required]
        public DateTimeOffset FromDate { get; set; }

        [Required]
        public DateTimeOffset ToDate { get; set; }

        public List<long> GuestIds { get; set; } 

        public int SequenceNo { get; set; }
    }
}
