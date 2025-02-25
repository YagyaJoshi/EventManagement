namespace EventManagement.DataAccess.ViewModels.Dtos
{
    public class SubscriptionPlanDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string PriceId { get; set; }
        public int Duration { get; set; }
        public decimal Amount { get; set; }
        public int CurrencyId { get; set; }
        public bool IsAccommodationEnabled { get; set; }
        public bool IsTicketingSystemEnabled { get; set; }
        public bool IsVisaEnabled { get; set; }
        public int NoOfEvents { get; set; }
        public int NoOfStaffs { get; set; }
        public string Status { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public bool IsAccreditationEnabled { get; set; }
        public string SubscriptionId {  get; set; }
        public bool IsPlanActive
        {
            get
            {
                // Try to parse the EndDate string into a DateTimeOffset
                if (DateTimeOffset.TryParse(EndDate, out var endDate))
                {
                    // Compare the parsed EndDate with the current UTC time
                    return endDate >= DateTimeOffset.UtcNow;
                }
                else
                {
                    // If parsing fails, return false (or handle accordingly)
                    return false;
                }
            }
        }
    }
}
