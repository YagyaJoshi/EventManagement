using EventManagement.DataAccess.ViewModels.Dtos;

namespace EventManagement.BusinessLogic.Services.v1.Abstractions
{
    public interface IReportsServices
    {
        Task<StatisticsReportDto> GetSummaryStatistics(long organizationId, long? eventId);

        Task<List<BookingReportItem>> GetBookingStatsPie(long organizationId, long? eventId, int year, int? month);

        Task<List<BarGraphReportDto>> GetBookingsBarGraphReport(long organizationId, long? eventId, int year, int? month);

        Task<OrganizationSummaryDto> GetOrganizationSummary(long? organizationId);

        Task<List<BarGraphReportDto>> GetBookingsRevenueReport(long? organizationId, int year, int? month);

        Task<PaymentSummaryDto> GetPaymentSummary(long organizationId, long? eventId, int? year, int? month, string date);
    }
}
