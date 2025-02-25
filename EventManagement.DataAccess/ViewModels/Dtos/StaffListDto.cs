using EventManagement.DataAccess.ViewModels.ApiObjects;

namespace EventManagement.DataAccess.ViewModels.Dtos
{
    public class StaffListDto
    {
        public List<StaffDetailsDto> List { get; set; } = new List<StaffDetailsDto>();
        public int TotalCount { get; set; }
        public int TotalRecords { get; set; }
    }
}
