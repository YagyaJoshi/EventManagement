using System.ComponentModel.DataAnnotations;

namespace EventManagement.DataAccess.ViewModels.ApiObjects
{
    public class CheckoutSessionInput
    {
        [Required(ErrorMessage = "SubscriptionPlanId is required")]
        public long SubscriptionPlanId { get; set; }

        [Required(ErrorMessage = "SuccessUrl is required")]
        public string SuccessUrl { get; set; }

        [Required(ErrorMessage = "CancelUrl is required")]
        public string CancelUrl { get; set; }
    }
}
