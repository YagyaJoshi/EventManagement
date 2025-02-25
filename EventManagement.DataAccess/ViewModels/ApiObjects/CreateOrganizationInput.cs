using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace EventManagement.DataAccess.ViewModels.ApiObjects
{
    public class CreateOrganizationInput
    {

        [Required(ErrorMessage = "Please enter name.")]
        public string organizationName { get; set; }

        [Required(ErrorMessage = "Please enter logo url.")]
        public IFormFile logo { get; set; }

        public Themes theme { get; set; }

        [Required(ErrorMessage = "Please enter an Email Id.")]
        public string email { get; set; }

        [Required(ErrorMessage = "Please enter phone")]
        public string phone { get; set; }

        [Required(ErrorMessage = "Please enter password")]
        [RegularExpression(@"^.{6,}$", ErrorMessage = "Password must have a minimum of 6 characters.")]

        public string password { get; set; }
    }

    public class Colors
    {
        [Required(ErrorMessage = "Primary color is required for the theme")]
        public string primary { get; set; }
        public string? secondary { get; set; } = string.Empty;
        public string? accent { get; set; } = string.Empty;
    }

    public class Themes
    {
        public Color color { get; set; }
    }
}
