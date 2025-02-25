namespace EventManagement.DataAccess.ViewModels.Dtos
{
    public class StatisticsReportDto
    {
        public long TotalBookings { get; set; }

        public decimal TotalRevenue { get; set; }

        public long TotalCountries { get; set; }

        public long TotalRegistrations { get; set; }

        public long TotalGuests { get; set; }
    }
}
