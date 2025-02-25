namespace EventManagement.DataAccess.ViewModels.Dtos
{
    public class AccreditationListDto
    {
        public List<AccreditationDto> List { get; set; } = new List<AccreditationDto>();
        public int TotalCount { get; set; }
    }
}
