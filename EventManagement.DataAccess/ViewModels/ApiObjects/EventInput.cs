using EventManagement.DataAccess.ViewModels.Dtos;
using System.ComponentModel.DataAnnotations;

namespace EventManagement.DataAccess.ViewModels.ApiObjects
{
    public class EventInput
    {
        public long Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string BannerImage { get; set; }
        [Required]
        public string Description { get; set; }
        public string Latitude { get; set; } = string.Empty;
        public string Longitude { get; set; } = string.Empty;
        [Required]
        public string Address { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string State { get; set; }
        [Required]
        public int CountryId { get; set; }
        [Required]
        public int TimeZoneId { get; set; }
        [Required]
        public DateTimeOffset StartDate { get; set; }
        [Required]
        public DateTimeOffset EndDate { get; set; }
        public string AccommodationInfoFile { get; set; }
        public string TransportationInfoFile { get; set; }
        public List<int> AccessibilityInfo { get; set; }
        public List<string> AccommodationPackageInfo { get; set; }
        public List<RoleWiseData> RoleWiseData { get; set; }
        public List<PaymentMethodSupported> PaymentMethodSupported { get; set; }
        public string Status { get; set; }
        public int? PaymentProviderId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public List<Penalties> Penalties { get; set; }
        public List<int> HotelIds { get; set; } = new List<int>();
    }

    public class RoleWiseData
    {
        [Required]
        public string Role { get; set; }
        [Required]
        public decimal Price { get; set; }
        public List<string> Access { get; set; }
        [Required]
        public int CurrencyId { get; set; }
        public string Code { get; set; }
    }

    public class PaymentMethodSupported
    {
        public string Label { get; set; }
        public string Type { get; set; }
        public bool Value { get; set; }
    }
}
