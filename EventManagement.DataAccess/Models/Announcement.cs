
using System.ComponentModel.DataAnnotations;

namespace EventManagement.DataAccess.Models
{
    public class Announcement
    {
        [Required]
        public string Image { get; set; }
        [Required]
        public string Heading { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public DateTimeOffset StartDate { get; set; }
        [Required]
        public DateTimeOffset EndDate { get; set;}
        [Required]
        public string Location { get; set; }
        [Required]
        public string Status { get; set; }
    }
}
