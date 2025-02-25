
namespace EventManagement.DataAccess.Models
{
    public class TransactionDetail
    {
        public long Id { get; set; }
        public long OrderId { get; set; }
        public long TransactionId { get; set; }
        public string TransactionDetails { get; set; }
        public int PaymentTypeId { get; set; }
        public DateTime PaymentDate { get; set; }
        public int Status { get; set; }
    }
}
