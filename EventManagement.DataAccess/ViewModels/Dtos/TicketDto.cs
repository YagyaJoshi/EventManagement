
using EventManagement.DataAccess.Models;

namespace EventManagement.DataAccess.ViewModels.Dtos
{
    public class TicketDto
    {
        public List<Ticket> List { get; set; } = new List<Ticket>();
        public int TotalCount { get; set; }
    }
}
