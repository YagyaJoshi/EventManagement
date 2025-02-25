using System.ComponentModel.DataAnnotations;

namespace EventManagement.DataAccess.ViewModels.ApiObjects
{
    public class OrganizationUpdateInput
    {
        [Required(ErrorMessage = "Please enter name.")]
        //[RegularExpression(@"^[a-zA-Z]+(\s[a-zA-Z]+)*$", ErrorMessage = "OrganizationName cannot contain spaces or special characters.")]
        public string OrganizationName { get; set; }

        [Required(ErrorMessage = "Please enter logo url.")]
        public string Logo { get; set; }

        public string BannerImage { get; set; } = string.Empty;

        public string BannerHeading { get; set; } = string.Empty;
        public string BannerSubHeading { get; set; } = string.Empty;
        public Theme Theme { get; set; }
        public string Website { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter an Email Id.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please enter phone")]
        public string Phone { get; set; }
        public string DomainName { get; set; } = string.Empty;
        [Required]
        public string Status { get; set; }

        public decimal VisaFees { get; set; }

    }

    public class OrganizationDetailsUpdateInput
    {
        [Required(ErrorMessage = "Please enter organization name.")]
        //[RegularExpression(@"^[a-zA-Z]+(\s[a-zA-Z]+)*$", ErrorMessage = "OrganizationName cannot contain spaces or special characters.")]
        public string OrganizationName { get; set; }

        [Required(ErrorMessage = "Please enter logo url.")]
        public string Logo { get; set; }

        public string BannerImage { get; set; }

        public string BannerHeading { get; set; } = string.Empty;

        public string BannerSubHeading { get; set; } = string.Empty;
        public Theme Theme { get; set; }
        public string Website { get; set; }

        [Required(ErrorMessage = "Please enter an Email Id.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please enter phone")]
        public string Phone { get; set; }
        public string DomainName { get; set; } = string.Empty;

        [Required]
        public int CurrencyId { get; set; }

        [Required]
        public int DisplayCurrencyId { get; set; } 

        [Required]
        public decimal DisplayCurrencyRate { get; set; }
        [Required]
        public decimal VisaFees { get; set; }
    }
}
