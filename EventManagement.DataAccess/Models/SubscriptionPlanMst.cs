using System.ComponentModel.DataAnnotations;

namespace EventManagement.DataAccess.Models
{
    public class SubscriptionPlanMst
    {
        [Required]
        public long Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string PriceId { get; set; }

        [Required]
        public int Duration { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public int CurrencyId {  get; set; }
        public bool IsAccommodationEnabled { get; set; }
        public bool IsTicketingSystemEnabled { get; set; }

        public bool IsVisaEnabled { get; set; }

        [Required]
        public int NoOfEvents { get; set; }

        [Required]

        public int NoOfStaffs { get; set; }
        public string Status { get; set; }

        public bool IsAccreditationEnabled { get; set; }
    }
}
