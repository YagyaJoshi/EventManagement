using System.ComponentModel.DataAnnotations;

namespace EventManagement.DataAccess.ViewModels.ApiObjects
{
    public class SignIn
    {
        [Required(ErrorMessage = "Please enter an Email Id.")]
        [EmailAddress(ErrorMessage = "Please enter a valid Email Id.")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; } = String.Empty;

        [Required(ErrorMessage = "Please enter your Password.")]
        [RegularExpression(@"^.{6,}$", ErrorMessage = "Password must have a minimum of 6 characters.")]

        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}
