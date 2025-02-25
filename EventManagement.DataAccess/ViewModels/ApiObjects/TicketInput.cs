
using System.ComponentModel.DataAnnotations;

namespace EventManagement.DataAccess.ViewModels.ApiObjects
{
    public class TicketInput
    {
        public long? TicketId { get; set; }
        [Required]
        public string Subject { get; set; }
        [Required]
        public string Message { get; set; }
    }
}
