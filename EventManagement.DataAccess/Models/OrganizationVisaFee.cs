
namespace EventManagement.DataAccess.Models
{
    public class OrganizationVisaFee
    {
        public long Id { get; set; }
        public long OrganizationId { get; set; }
        public long CountryId { get; set; }
        public decimal? Fees { get; set; }
    }
}
