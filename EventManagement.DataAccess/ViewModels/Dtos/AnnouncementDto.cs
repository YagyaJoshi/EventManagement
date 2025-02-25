

namespace EventManagement.DataAccess.ViewModels.Dtos
{
    public class AnnouncementDto
    {
        public long Id { get; set; }
        public long OrganizationId { get; set; }
        public string Image { get; set; }
        public string Heading { get; set; }
        public string Description { get; set; }
        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
        public string Location { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
