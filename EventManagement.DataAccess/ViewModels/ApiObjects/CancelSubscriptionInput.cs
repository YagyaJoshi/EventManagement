using System.ComponentModel.DataAnnotations;

namespace EventManagement.DataAccess.ViewModels.ApiObjects
{
    public class CancelSubscriptionInput
    {
        [Required(ErrorMessage = "SubscriptionId is required")]
        public string SubscriptionId { get; set; }
    }
}
