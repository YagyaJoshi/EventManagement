namespace EventManagement.DataAccess.ViewModels.Dtos
{
    public class AccomodationInfoDto
    {
        public long OrderDetailId { get; set; }
        public long OrderId { get; set; }
        public List<int> GuestIds { get; set; }

        public long HotelId { get; set; }
        public long HotelRoomTypeId { get; set; }
        public decimal Amount { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public int NumberOfNights { get; set; }
        public int? SequenceNo { get; set; }
        public List<string> GuestNames { get; set; }

        public HotelDetailsDto Hotel {  get; set; }
    }

}
