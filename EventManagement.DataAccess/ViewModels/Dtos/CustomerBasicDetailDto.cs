namespace EventManagement.DataAccess.ViewModels.Dtos
{
    public class CustomerBasicDetailDto
    {
        public long UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public long OrganizationId { get; set; }
        public string OrganizationName { get; set; }
        public string MerchantId { get; set; }
        public string ApiPassword { get; set; }

        public decimal DisplayCurrencyRate { get; set; }

        public string CurrencyCode { get; set; }

        public string PaymentUrl { get; set; }

        public int ApiVersion { get; set; }
    }
}
