using System.ComponentModel.DataAnnotations;

namespace EventManagement.DataAccess.ViewModels.ApiObjects
{
    public class AddStaffInput
    {
        public long Id { get; set; }

        [Required]
        //[RegularExpression(@"^[a-zA-Z]+(\s[a-zA-Z]+)*$", ErrorMessage = "FirstName cannot contain spaces or special characters.")]
        public string FirstName { get; set; }

        [Required]
        //[RegularExpression(@"^[a-zA-Z]+(\s[a-zA-Z]+)*$", ErrorMessage = "LastName cannot contain spaces or special characters.")]
        public string LastName { get; set; }

        [Required]
        [Phone]
        [DataType(DataType.PhoneNumber)]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Please enter an Email Id.")]
        [EmailAddress(ErrorMessage = "Please enter a valid Email Id.")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required]
        public string Status { get; set; }

        [RegularExpression(@"^.{6,}$", ErrorMessage = "Password must have a minimum of 6 characters.")]
        public string Password { get; set; } = string.Empty;

        [Required]
        public int RoleId { get; set; }
    }
}
    