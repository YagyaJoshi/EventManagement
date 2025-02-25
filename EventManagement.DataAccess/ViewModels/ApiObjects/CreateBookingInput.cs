using System.ComponentModel.DataAnnotations;

namespace EventManagement.DataAccess.ViewModels.ApiObjects
{
    public class CreateBookingInput
    {
        [Required(ErrorMessage = "OrderId is required")]
        public long OrderId { get; set; }

        public bool UseWalletBalance { get; set; }
        public string PaymentType { get; set; }

        [Required(ErrorMessage = "SuccessUrl is required")]
        public string SuccessUrl { get; set; }

        [Required(ErrorMessage = "CancelUrl is required")]
        public string CancelUrl { get; set; }

        public bool IsPayingUnpaidAmount { get; set; } = false;
    }
}
