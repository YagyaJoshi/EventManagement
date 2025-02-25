using EventManagement.DataAccess.Models;

namespace EventManagement.DataAccess.ViewModels.Dtos
{
    public class BookingListDto
    {
        public List<Orders> List { get; set; } = new List<Orders>();
        public int TotalCount { get; set; }
    }

}
