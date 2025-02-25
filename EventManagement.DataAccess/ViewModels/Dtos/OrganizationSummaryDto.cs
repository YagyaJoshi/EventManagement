namespace EventManagement.DataAccess.ViewModels.Dtos
{
    public class OrganizationSummaryDto
    {
        public long TotalOrganizations { get; set; }

        public long TotalCustomers { get; set; }

        public decimal TotalRevenue { get; set; }

        public long TotalEvents { get; set; }
    }
}
