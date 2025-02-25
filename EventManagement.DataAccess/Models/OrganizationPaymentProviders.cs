namespace EventManagement.DataAccess.Models
{
    public class OrganizationPaymentProviders
    {
        public long Id { get; set; }

        public long OrganizationId { get; set; }
        public string PaymentMethod { get; set; }
        public string MerchantId { get; set; } = string.Empty;
        public string ApiPassword { get; set; } = string.Empty;
        public string BankDetails { get; set; } = string.Empty;
        public bool IsProduction { get; set; } 
        public DateTime CreatedDate { get; set; } 
        public DateTime? UpdatedDate { get; set; } 

        public string PaymentUrl { get; set; }

        public int ApiVersion { get; set; }
    }
}
