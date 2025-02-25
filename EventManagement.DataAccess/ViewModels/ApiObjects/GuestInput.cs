
using System.ComponentModel.DataAnnotations;

namespace EventManagement.DataAccess.ViewModels.ApiObjects
{
    public class GuestInput
    {
        [Required(ErrorMessage = "EventId is required.")]
        public long EventId { get; set; }

        [Required(ErrorMessage = "PassportFirstName is required.")]
        //[RegularExpression(@"^[a-zA-Z]+(\s[a-zA-Z]+)*$", ErrorMessage = "PassportFirstName cannot contain spaces or special characters.")]
        public string PassportFirstName { get; set; }

        [Required(ErrorMessage = "PassportLastName is required.")]
        //[RegularExpression(@"^[a-zA-Z]+(\s[a-zA-Z]+)*$", ErrorMessage = "PassportLastName cannot contain spaces or special characters.")]
        public string PassportLastName { get; set; }

        [Required]
        public string Role { get; set; } = string.Empty;
        public string PassportNumber { get; set; } = string.Empty;
    }
}
