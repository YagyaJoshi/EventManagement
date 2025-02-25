using EventManagement.DataAccess.ViewModels.ApiObjects;
using EventManagement.DataAccess.ViewModels.Dtos;

namespace EventManagement.DataAccess.Models
{
    public class Organization
    {
        public long Id { get; set; }
        public string OrganizationName { get; set; }
        public string Logo { get; set; }
        public string BannerImage { get; set; }
        public string BannerHeading { get; set; }
        public string BannerSubHeading { get; set; }
        public Theme Theme { get; set; }
        public string Website { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string DomainName { get; set; }
        public string Status { get; set; }
        public CurrencyDto DisplayCurrency { get; set; }

        public CurrencyDto DefaultCurrency { get; set; }

        public decimal? DisplayCurrencyRate { get; set; }

        public decimal? VisaFees { get; set; }
    }
 }
