
namespace EventManagement.DataAccess.ViewModels.ApiObjects
{
    public class TransactionDetailInput
    {
        public long OrderId { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public string TransactionDetails { get; set; } = string.Empty;
        public string PaymentType { get; set; }
        public decimal Amount { get; set; }
        public decimal WalletAmount { get; set; }
        
        public decimal PenaltyAmount { get; set; }
        public long OrganizationId { get; set; }
    }
}
