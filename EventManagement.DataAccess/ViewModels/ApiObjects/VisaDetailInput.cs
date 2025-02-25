using EventManagement.DataAccess.Enums;
using System.ComponentModel.DataAnnotations;

namespace EventManagement.DataAccess.ViewModels.ApiObjects
{
    public class VisaDetailInput
    {
        [Required]
        public long OrderId { get; set; }
        public List<GuestVisaInfo> GuestVisaInfos { get; set; }
    }

    public class GuestVisaInfo
    {
        [Required]
        public int GuestId { get; set;}
        [Required]
        public bool VisaAssistanceRequired { get; set;}
        [Required]
        public bool VisaOfficialLetterRequired { get; set;}

        public int VisaStatus {  get; set;}
    }
}
