using System.ComponentModel.DataAnnotations;

namespace EventManagement.DataAccess.ViewModels.ApiObjects
{
    public class VisaStatusInput
    {
        [Required]
        public string VisaStatus { get; set; }
    }
}
