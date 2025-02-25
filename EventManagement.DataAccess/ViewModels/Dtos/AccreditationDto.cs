namespace EventManagement.DataAccess.ViewModels.Dtos
{
    public class AccreditationDto
    {
        public long Id { get; set; }
        public long OrganizationId { get; set; }
        public string TemplateImage { get; set; }
        public string Instruction { get; set; }
        public string Status { get; set; }
    }
}
