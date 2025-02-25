
namespace EventManagement.DataAccess.ViewModels.ApiObjects
{
    public class OrganizationPaymentProviderInput
    {
        public long Id { get; set; }
        public string PaymentMethod { get; set; }
        public string MerchantId { get; set; } = string.Empty;
        public string ApiPassword { get; set; } = string.Empty;
        public string BankDetails { get; set; } = string.Empty;
        public bool IsProduction { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
