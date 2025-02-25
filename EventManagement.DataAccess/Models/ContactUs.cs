
using System.ComponentModel.DataAnnotations;

namespace EventManagement.DataAccess.Models
{
    public class ContactUs
    {
        //public long Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Message { get; set; }
    }
}
