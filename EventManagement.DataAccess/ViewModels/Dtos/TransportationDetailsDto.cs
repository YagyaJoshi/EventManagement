namespace EventManagement.DataAccess.ViewModels.Dtos
{
    public class TransportationListDto
    {
        public List<GuestDetailDto> List { get; set; } = new List<GuestDetailDto>();

        public int TotalCount { get; set; }
    }
}
