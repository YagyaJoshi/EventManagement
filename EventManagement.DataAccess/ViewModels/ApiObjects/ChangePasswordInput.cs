using System.ComponentModel.DataAnnotations;

namespace EventManagement.DataAccess.ViewModels.ApiObjects
{
    public class ChangePasswordInput
    {
        [Required(ErrorMessage = "UserId is required.")]
        public long UserId { get; set; }

        [Required(ErrorMessage = "Old Password is required.")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "Please enter your Password.")]
        [RegularExpression(@"^.{6,}$", ErrorMessage = "Password must have a minimum of 6 characters.")]

        public string Password { get; set; }

        [Required]
        [Compare(nameof(Password), ErrorMessage = "Password and Confirm Password fields must be identical. Please try again.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
    }
}
