
namespace EventManagement.DataAccess.ViewModels.ApiObjects
{
    public class TransportationInput
    {
        public long GuestId { get; set; }
        public long OrderId { get; set; }
        public string PassportNumber { get; set; } = string.Empty;
        public DateTimeOffset? PassportIssueDate { get; set; } = null;
        public DateTimeOffset? PassportExpiryDate { get; set; } = null;

        public DateTimeOffset? DOB { get; set; } = null;
        public string Occupation { get; set; } = string.Empty;
        public string Nationality { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;
        public string WorkPlace { get; set; } = string.Empty;
        public string DepartureFlightAirport { get; set; } = string.Empty;
        public string ArrivalFlightAirport { get; set; } = string.Empty;
        public string ArrivalFlightNumber { get; set; } = string.Empty;
        public string DepartureFlightNumber { get; set; } = string.Empty;
        public DateTimeOffset? ArrivalDateTime { get; set; } = null;
        public DateTimeOffset? DepartureDateTime { get; set; } = null;
        public string ArrivalNotes { get; set; } = string.Empty;
        public string DepartureNotes { get; set; } = string.Empty;
        public List<int> AccessibilityInfo { get; set; } = new List<int>();
        public string Photo { get; set; } = string.Empty;
        public string VisaDocument {  get; set; } = string.Empty;
        public string VisaStatus { get; set; } = string.Empty;

        public string VisaOfficialLetterDocument {  get; set; } = string.Empty;

        public string PassportImage { get; set; } = string.Empty;

        // Method to check if all required fields are filled
        public bool IsCompleted()
        {
            return !string.IsNullOrWhiteSpace(PassportNumber) &&
                   PassportIssueDate != default &&
                   PassportExpiryDate != default &&
                   DOB != default &&
                   !string.IsNullOrWhiteSpace(Nationality) &&
                   !string.IsNullOrWhiteSpace(Occupation) &&
                   !string.IsNullOrWhiteSpace(JobTitle) &&
                   !string.IsNullOrWhiteSpace(WorkPlace) &&
                   !string.IsNullOrWhiteSpace(DepartureFlightAirport) &&
                   !string.IsNullOrWhiteSpace(ArrivalFlightAirport) &&
                   !string.IsNullOrWhiteSpace(ArrivalFlightNumber) &&
                   !string.IsNullOrWhiteSpace(DepartureFlightNumber) &&
                   !string.IsNullOrWhiteSpace(Photo) &&
                   ArrivalDateTime != null &&
                   DepartureDateTime != null &&
                   !string.IsNullOrWhiteSpace(PassportImage);
        }
    }
}
