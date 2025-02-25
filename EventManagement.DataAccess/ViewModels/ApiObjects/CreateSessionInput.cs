using System.ComponentModel.DataAnnotations;

namespace EventManagement.DataAccess.ViewModels.ApiObjects
{
    public class CreateSessionInput
    {
        [Required(ErrorMessage = "OrderId is required")]
        public long OrderId { get; set; }

        public bool UseWalletBalance { get; set; }

        [Required(ErrorMessage = "SuccessUrl is required")]
        public string SuccessUrl { get; set; }

        [Required(ErrorMessage = "CancelUrl is required")]
        public string CancelUrl { get; set; }
    }

    public class CreateSessionForWalletInput
    {
        [Required(ErrorMessage = "Amount is required")]
        // [Range(1, double.MaxValue, ErrorMessage = "Amount must be at least 1")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "SuccessUrl is required")]
        public string SuccessUrl { get; set; }

        [Required(ErrorMessage = "CancelUrl is required")]
        public string CancelUrl { get; set; }
    }
}
