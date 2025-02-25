namespace EventManagement.DataAccess.ViewModels.Dtos
{
    public class OrderDto
    {
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string TransactionId { get; set; }
    }
}
