using EventManagement.BusinessLogic.Services.v1.Abstractions;
using EventManagement.DataAccess;
using EventManagement.DataAccess.ViewModels.Dtos;
using System.Data.SqlClient;
using System.Data;
using Microsoft.Extensions.Configuration;
using EventManagement.DataAccess.Enums;
using EventManagement.DataAccess.Extensions;
using EventManagement.BusinessLogic.Exceptions;
using EventManagement.BusinessLogic.Resources;

namespace EventManagement.BusinessLogic.Services.v1.Implementations
{
    public class ReportsServices : IReportsServices
    {
        private readonly IConfiguration _configuration;
        public ReportsServices(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<StatisticsReportDto> GetSummaryStatistics(long organizationId, long? eventId)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetSummaryStatistics");

            try
            {
                objCmd.Parameters.AddWithValue("@OrganizationId", organizationId);
                if(eventId > 0)
                    objCmd.Parameters.AddWithValue("@EventId", eventId);
                DataSet ds = await objSQL.FetchDB(objCmd);

                var summary = (from DataRow dr in ds.Tables[0].Rows
                              select new StatisticsReportDto
                              {
                                  TotalBookings = Convert.ToInt64(dr["TotalBookings"]),
                                  TotalRevenue = Convert.ToDecimal(dr["TotalRevenue"]),
                                  TotalCountries = Convert.ToInt64(dr["TotalCountries"]),
                                  TotalRegistrations = Convert.ToInt64(dr["TotalRegistrations"]),
                                  TotalGuests = Convert.ToInt64(dr["TotalGuests"])
                              }).FirstOrDefault();

                return summary;
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (objSQL != null) objSQL.Dispose();
                if (objCmd != null) objCmd.Dispose();
            }
        }
        public async Task<List<BookingReportItem>> GetBookingStatsPie(long organizationId, long? eventId, int year, int? month)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetBookingStatsPie");

            try
            {
                if (year <= 0)
                    throw new ServiceException(Resource.YEAR_REQUIRED);

                objCmd.Parameters.AddWithValue("@OrganizationId", organizationId);
                objCmd.Parameters.AddWithValue("@EventId", eventId > 0 ? eventId : DBNull.Value);
                objCmd.Parameters.AddWithValue("@Year", year);
                objCmd.Parameters.AddWithValue("@Month", month > 0 ? month : DBNull.Value);
                DataTable dt = await objSQL.FetchDT(objCmd);

                var summary = (from DataRow dr in dt.Rows
                               select new BookingReportItem
                               {
                                  Status = StatusExtensions.ToStatusString((Status)Convert.ToInt32(dr["Status"])),
                                  Total = Convert.ToInt32(dr["Total"]),
                                  Percentage = Convert.ToDouble(dr["Percentage"])
                               }).ToList();

                return summary;
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (objSQL != null) objSQL.Dispose();
                if (objCmd != null) objCmd.Dispose();
            }
        }

        public async Task<List<BarGraphReportDto>> GetBookingsBarGraphReport(long organizationId, long? eventId, int year, int? month)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetBookingsBarGraphReport");

            try
            {
                if (year <= 0)
                    throw new ServiceException(Resource.YEAR_REQUIRED);

                objCmd.Parameters.AddWithValue("@OrganizationId", organizationId);
                if (eventId > 0)
                    objCmd.Parameters.AddWithValue("@EventId", eventId);
                objCmd.Parameters.AddWithValue("@Year", year);
                objCmd.Parameters.AddWithValue("@Month", month > 0 ? month : DBNull.Value);

                DataTable dt = await objSQL.FetchDT(objCmd);

                var summary = (from DataRow dr in dt.Rows
                               select new BarGraphReportDto
                               {
                                  Value = Convert.ToInt32(dr["Value"]),
                                  Label = Convert.ToString(dr["Label"])
                               }).ToList();

                return summary;
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (objSQL != null) objSQL.Dispose();
                if (objCmd != null) objCmd.Dispose();
            }
        }

        public async Task<OrganizationSummaryDto> GetOrganizationSummary(long? organizationId)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetOrganizationSummary");

            try
            {
                if(organizationId > 0)
                    objCmd.Parameters.AddWithValue("@OrganizationId", organizationId);
                DataTable dt = await objSQL.FetchDT(objCmd);

                var summary = (from DataRow dr in dt.Rows
                               select new OrganizationSummaryDto
                               {
                                   TotalOrganizations = Convert.ToInt64(dr["TotalOrganizations"]),
                                   TotalRevenue = Convert.ToDecimal(dr["TotalRevenue"]),
                                   TotalCustomers = Convert.ToInt64(dr["TotalCustomers"]),
                                   TotalEvents = Convert.ToInt64(dr["TotalEvents"])
                               }).FirstOrDefault();

                return summary;
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (objSQL != null) objSQL.Dispose();
                if (objCmd != null) objCmd.Dispose();
            }
        }

        public async Task<List<BarGraphReportDto>> GetBookingsRevenueReport(long? organizationId, int year, int? month)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetBookingsRevenueReport");

            try
            {
                if (year <= 0)
                    throw new ServiceException(Resource.YEAR_REQUIRED);

                if (organizationId > 0)
                    objCmd.Parameters.AddWithValue("@OrganizationId", organizationId);
                objCmd.Parameters.AddWithValue("@Year", year);
                objCmd.Parameters.AddWithValue("@Month", month > 0 ? month : DBNull.Value);

                DataTable dt = await objSQL.FetchDT(objCmd);

                var summary = (from DataRow dr in dt.Rows
                               select new BarGraphReportDto
                               {
                                   Value = Convert.ToInt32(dr["Value"]),
                                   Label = Convert.ToString(dr["Label"])
                               }).ToList();

                return summary;
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (objSQL != null) objSQL.Dispose();
                if (objCmd != null) objCmd.Dispose();
            }
        }

        public async Task<PaymentSummaryDto> GetPaymentSummary(long organizationId, long? eventId, int? year, int? month, string date)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetPaymentSummaryReport");

            try
            {
                DateTime parsedDate;
                objCmd.Parameters.AddWithValue("@OrganizationId", organizationId);
                objCmd.Parameters.AddWithValue("@EventId", eventId > 0 ? eventId : DBNull.Value);
                objCmd.Parameters.AddWithValue("@Year", year > 0 ? year  : DBNull.Value);
                objCmd.Parameters.AddWithValue("@Month", month > 0 ? month : DBNull.Value);
                objCmd.Parameters.AddWithValue("@Date", DateTime.TryParse(date, out parsedDate) ? (object)parsedDate : DBNull.Value);

                DataTable dt = await objSQL.FetchDT(objCmd);

                var summary = (from DataRow dr in dt.Rows
                               select new PaymentSummaryDto
                               {
                                   All = new PaymentSummary()
                                   {
                                       TotalAmount = Convert.ToDecimal(dr["AllTotalAmount"]),
                                       TotalOrders = Convert.ToInt64(dr["AllTotalOrders"])
                                   },
                                   Completed = new PaymentSummary()
                                   {
                                       TotalAmount = Convert.ToDecimal(dr["CompletedTotalAmount"]),
                                       TotalOrders = Convert.ToInt64(dr["CompletedTotalOrders"])
                                   },
                                   Pending = new PaymentSummary()
                                   {
                                       TotalAmount = Convert.ToDecimal(dr["PendingTotalAmount"]),
                                       TotalOrders = Convert.ToInt64(dr["PendingTotalOrders"])
                                   }
                               }).FirstOrDefault();

                return summary;
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (objSQL != null) objSQL.Dispose();
                if (objCmd != null) objCmd.Dispose();
            }
        }
    }
}
