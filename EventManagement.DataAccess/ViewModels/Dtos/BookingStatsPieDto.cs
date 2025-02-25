namespace EventManagement.DataAccess.ViewModels.Dtos
{
    public class BookingStatsPieDto
    {
        public int TotalBookings { get; set; }
        public List<BookingReportItem> BookingReport { get; set; }
    }

    public class BookingReportItem
    {
        public string Status { get; set; }
        public int Total { get; set; }
        public double Percentage { get; set; }
    }
}
