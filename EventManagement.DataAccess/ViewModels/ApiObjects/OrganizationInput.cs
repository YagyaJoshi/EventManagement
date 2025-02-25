using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace EventManagement.DataAccess.ViewModels.ApiObjects
{
    public class OrganizationInput
    {

        [Required(ErrorMessage = "Please enter name.")]
        public string organizationName { get; set; }

        [Required(ErrorMessage = "Please enter logo url.")]
        public IFormFile logo { get; set; }

        public IFormFile? bannerImage { get; set; }

        public string? defaultBannerImage { get; set; } = null;
        public Theme theme { get; set; }
        public string? website { get; set; } = string.Empty;

        public string? bannerHeading {  get; set; } = string.Empty;
        public string? bannerSubHeading {  get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter an Email Id.")]
        public string email { get; set; }

        [Required(ErrorMessage = "Please enter phone")]
        public string phone { get; set; }
        public string? domainName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter password")]
        [RegularExpression(@"^.{6,}$", ErrorMessage = "Password must have a minimum of 6 characters.")]

        public string password { get; set; }

        [Required]
        public string status { get; set; }
    }

    public class Color
    {
        [Required(ErrorMessage = "Please choose theme")]
        public string primary { get; set; }
        public string? secondary { get; set; } = string.Empty;
        public string? accent { get; set; } = string.Empty;
    }

    public class Theme
    {
        public Color color { get; set; }
    }

}
