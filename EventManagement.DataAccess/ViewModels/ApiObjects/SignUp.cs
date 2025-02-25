using EventManagement.DataAccess.Enums;
using System.ComponentModel.DataAnnotations;

namespace EventManagement.DataAccess.ViewModels.ApiObjects
{
    public class SignUp
    {
        [Required(ErrorMessage = "Please enter your First Name.")]
        [StringLength(50, ErrorMessage = "First Name cannot be longer than 50 characters.")]
        //[RegularExpression(@"^[a-zA-Z]+(\s[a-zA-Z]+)*$", ErrorMessage = "First Name cannot contain special characters or trailing spaces.")]
        public string FirstName { get; set; }


        [Required(ErrorMessage = "Please enter your First Name.")]
        [StringLength(50, ErrorMessage = "First Name cannot be longer than 50 characters.")]
        //[RegularExpression(@"^[a-zA-Z]+(\s[a-zA-Z]+)*$", ErrorMessage = "Last Name cannot contain special characters or trailing spaces.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Please enter your Mobile.")]
        [Phone]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Please enter an Email Id.")]
        [EmailAddress(ErrorMessage = "Please enter a valid Email Id.")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please enter country name")]
        public int CountryId { get; set; }

        [Required(ErrorMessage = "Please enter your Password.")]
        [RegularExpression(@"^.{6,}$", ErrorMessage = "Password must have a minimum of 6 characters.")]

        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [Compare("Password", ErrorMessage = "Password and Confirm Password fields must be identical. Please try again.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        [Required]
        public int CustomerOrganizationTypeId { get; set; }

        [Required]
        public string CustomerOrganizationName { get; set; }
    }
}
