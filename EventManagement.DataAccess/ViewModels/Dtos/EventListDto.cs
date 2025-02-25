using EventManagement.DataAccess.ViewModels.ApiObjects;

namespace EventManagement.DataAccess.ViewModels.Dtos
{
    public class EventListDto
    {
        public List<EventInfoDto> List { get; set; }
        public int TotalCount { get; set; }
        public int TotalRecords { get; set; }
    }
}
