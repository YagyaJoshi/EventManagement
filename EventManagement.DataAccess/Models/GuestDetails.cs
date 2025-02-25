
using EventManagement.DataAccess.ViewModels.Dtos;

namespace EventManagement.DataAccess.Models
{
    public class GuestDetails
    {
        public long Id { get; set; }
        public long OrderId { get; set; }
        public string PassportFirstName { get; set; }
        public string PassportLastName { get; set; }
        public string PassportNumber { get; set; }
        public DateTimeOffset PassportIssueDate { get; set; }
        public DateTimeOffset PassportExpiryDate { get; set; }
        public string Role {  get; set; }
        public DateTime DOB { get; set; }
        public string Occupation { get; set; }
        public string Nationality { get; set; }
        public string JobTitle { get; set; }
        public string WorkPlace { get; set; }
        public string DepartureFlightAirport { get; set; }
        public string ArrivalFlightAirport { get; set; }
        public string ArrivalFlightNumber { get; set; }
        public string DepartureFlightNumber { get; set; }
        public DateTimeOffset? ArrivalDateTime { get; set; }
        public DateTimeOffset? DepartureDateTime { get; set; }
        public string ArrivalNotes { get; set; }
        public string DepartureNotes { get; set; }
        public List<int> AccessibilityInfo { get; set; }
        public List<AccessiblityInfoDto> AccessibilityInfoData { get; set; } = new List<AccessiblityInfoDto>();
        public long? HotelId { get; set; }
        public long? HotelRoomTypeId { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public bool VisaAssistanceRequired { get; set; }
        public bool VisaOfficialLetterRequired { get; set; }
        public string VisaDocument { get; set; }
        public int? SequenceNo { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        public decimal RegistrationFee { get; set; }
    }
}
