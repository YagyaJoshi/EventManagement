using EventManagement.DataAccess.ViewModels.Dtos;
using System.ComponentModel.DataAnnotations;

namespace EventManagement.DataAccess.ViewModels.ApiObjects
{
    public class UpdateUserInput
    {
        [Required]
        //[RegularExpression(@"^[a-zA-Z]+(\s[a-zA-Z]+)*$", ErrorMessage = "FirstName cannot contain spaces or special characters.")]
        public string FirstName { get; set; }
        [Required]
        //[RegularExpression(@"^[a-zA-Z]+(\s[a-zA-Z]+)*$", ErrorMessage = "LastName cannot contain spaces or special characters.")]
        public string LastName { get; set; }

        [Required]
        public string Phone { get; set; }
        public int CountryId { get; set; }
        public int? CustomerOrganizationTypeId { get; set; }
        public string CustomerOrganizationName { get; set; } = string.Empty;
    }
}
