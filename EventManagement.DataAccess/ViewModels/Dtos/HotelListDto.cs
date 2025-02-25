
namespace EventManagement.DataAccess.ViewModels.Dtos
{
    public class HotelListDto
    {
        public List<HotelDto> List { get; set; } = new List<HotelDto>();
        public int TotalCount { get; set; }
    }
}
