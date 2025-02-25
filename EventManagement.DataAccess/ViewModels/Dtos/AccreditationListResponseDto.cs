namespace EventManagement.DataAccess.ViewModels.Dtos
{
    public class AccreditationListResponseDto
    {
        public List<AccreditationListData> List { get; set; } = new List<AccreditationListData>();
        public int TotalCount { get; set; }
    }

    public class AccreditationListData
    {

        public long Id { get; set; }
        public long OrganizationId { get; set; }

        public long EventId { get; set; }

        public string EventName { get; set; }

        public string PassportFirstName { get; set; }

        public string PassportLastName { get; set; }

        public string PassportNumber { get; set; }

        public string Role { get; set; }
        public string Status { get; set; }

        public string Photo {  get; set; }

        public string VisaStatus { get; set; }
    }
}
