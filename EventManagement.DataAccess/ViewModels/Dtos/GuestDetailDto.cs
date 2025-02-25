
using EventManagement.DataAccess.Enums;

namespace EventManagement.DataAccess.ViewModels.Dtos
{
    public class GuestDetailDto
    {
        public long Id { get; set; }
        public long OrderId { get; set; }
        public long EventId { get; set; }
        public string PassportFirstName { get; set; }
        public string PassportLastName { get; set; }
        public string PassportNumber { get; set; }
        public string PassportIssueDate { get; set; }
        public string PassportExpiryDate { get; set; }
        public string Role { get; set; }
        public string? DOB { get; set; }
        public string Occupation { get; set; }
        public string Nationality { get; set; }
        public string JobTitle { get; set; }
        public string WorkPlace { get; set; }
        public string DepartureFlightAirport { get; set; }
        public string ArrivalFlightAirport { get; set; }
        public string ArrivalFlightNumber { get; set; }
        public string DepartureFlightNumber { get; set; }
        public string ArrivalDateTime { get; set; }
        public string DepartureDateTime { get; set; }
        public string ArrivalNotes { get; set; }
        public string DepartureNotes { get; set; }
        public List<int> AccessibilityInfo { get; set; }
        public List<AccessiblityInfoDto> AccessibilityInfoData { get; set; } = new List<AccessiblityInfoDto>();
        public long? HotelId { get; set; }
        public long? HotelRoomTypeId { get; set; }
        public HotelDetailsDto Hotel { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public bool VisaAssistanceRequired { get; set; }
        public bool VisaOfficialLetterRequired { get; set; }
        public string VisaDocument { get; set; }
        public string Status { get; set; }
        public decimal RegistrationFee { get; set; }
        public string VisaStatus { get; set; }

        public string VisaOfficialLetterDocument { get; set; }
        public string Photo { get; set; }
        public int? SequenceNo { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string TimeZone { get; set; }

        public string PassportImage { get; set; }
    }
}
