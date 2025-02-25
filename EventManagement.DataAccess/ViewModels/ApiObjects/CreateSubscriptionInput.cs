using System.ComponentModel.DataAnnotations;

namespace EventManagement.DataAccess.ViewModels.ApiObjects
{
    public class CreateSubscriptionInput
    {
        [Required(ErrorMessage = "SessionId is required")]
        public string SessionId { get; set; }
    }
}
