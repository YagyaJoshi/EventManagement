namespace EventManagement.DataAccess.Models
{
    public class OrganizationSubscription
    {
        public long Id { get; set; }
        public long OrganizationId { get; set; }
        public long SubscriptionPlanId { get; set; }
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }
        public string CustomerId { get; set; }
        public string SubscriptionId { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
