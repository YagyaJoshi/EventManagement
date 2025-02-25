using EventManagement.DataAccess.Enums;

namespace EventManagement.DataAccess.ViewModels.ApiObjects
{
    public class AddPaymentRequest
    {
        public int OrderId { get; set; }

        public int TransactionId { get; set; }

        public string TransactionDetail { get; set; }

        public PaymentType PaymentTypeId { get; set; }

        public DateTime PaymentDate { get; set; }

        public PaymentStatus Status { get; set; }

    }
}
