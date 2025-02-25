namespace EventManagement.DataAccess.ViewModels.Dtos
{
    public class PaymentSummaryDto
    {
        public PaymentSummary All { get; set; }
        public PaymentSummary Completed { get; set; }
        public PaymentSummary Pending { get; set; }
    }

    public class PaymentSummary
    {
        public decimal TotalAmount { get; set; }
        public long TotalOrders { get; set; }
    }
 
}
