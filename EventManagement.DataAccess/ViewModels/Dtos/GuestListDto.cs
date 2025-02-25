
using EventManagement.DataAccess.Models;

namespace EventManagement.DataAccess.ViewModels.Dtos
{
    public class GuestListDto
    {
        public List<GuestDetailDto> List { get; set; } = new List<GuestDetailDto>();
        public int TotalCount { get; set; }
    }
}
