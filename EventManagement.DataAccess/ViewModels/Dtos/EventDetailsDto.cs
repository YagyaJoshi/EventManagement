using EventManagement.DataAccess.ViewModels.ApiObjects;

namespace EventManagement.DataAccess.ViewModels.Dtos
{
    public class EventDetailsDto
    {
        public long Id { get; set; }
        public long OrganizationId { get; set; }
        public string Name { get; set; }
        public string BannerImage { get; set; }
        public string Description { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public int? CountryId { get; set; }
        public string Country { get; set; }
        public int TimeZoneId { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string AccommodationInfoFile { get; set; }
        public string TransportationInfoFile { get; set; }
        public List<int> AccessibilityInfo { get; set; }

        public List<AccessiblityInfoDto> AccessibilityInfoData { get; set; } = new List<AccessiblityInfoDto>();
        public List<string> AccommodationPackageInfo { get; set; }
        public List<RoleWiseData> RoleWiseData { get; set; }
        public List<SupportedPaymentMethod> PaymentMethodSupported { get; set; }
        public int? PaymentproviderId { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public List<Penalties> Penalties { get; set; } = new List<Penalties>();
        public List<HotelDto> Hotels { get; set; } = new List<HotelDto>();
    }

    public class SupportedPaymentMethod {
        public string Label { get; set; }
        public string Type { get; set; }
        public bool Value { get; set; }
        public string Data { get; set; } = string.Empty;
    }

    public class EventDetailsDtov1
    {
        public long Id { get; set; }
        public long OrganizationId { get; set; }
        public string Name { get; set; }
        public string BannerImage { get; set; }
        public string Description { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public int? CountryId { get; set; }
        public string Country { get; set; }
        public int TimeZoneId { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string AccommodationInfoFile { get; set; }
        public string TransportationInfoFile { get; set; }
        public List<int> AccessibilityInfo { get; set; }

        public List<AccessiblityInfoDto> AccessibilityInfoData { get; set; } = new List<AccessiblityInfoDto>();
        public List<string> AccommodationPackageInfo { get; set; }
        public List<RoleWiseData> RoleWiseData { get; set; }
        public List<SupportedPaymentMethod> PaymentMethodSupported { get; set; }
        public int? PaymentproviderId { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public List<Penalties> Penalties { get; set; } = new List<Penalties>();
    }
}
