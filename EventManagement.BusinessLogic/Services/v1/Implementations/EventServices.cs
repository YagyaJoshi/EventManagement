using EventManagement.BusinessLogic.Exceptions;
using EventManagement.BusinessLogic.Services.v1.Abstractions;
using EventManagement.DataAccess;
using EventManagement.DataAccess.ViewModels.ApiObjects;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Data;
using EventManagement.DataAccess.ViewModels.Dtos;
using Newtonsoft.Json;
using EventManagement.BusinessLogic.Helpers;
using EventManagement.DataAccess.Extensions;
using EventManagement.BusinessLogic.Services.v1.Mappings;
using EventManagement.DataAccess.Enums;
using EventManagement.Utilities.Helpers;
using EventManagement.DataAccess.Models;
using Stripe;

namespace EventManagement.BusinessLogic.Services.v1.Implementations
{
    public class EventServices : IEventServices
    {
        private readonly IConfiguration _configuration;
        public EventServices(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<long> AddEvent(long? id, long organizationId, EventInput input)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_AddOrUpdateEvent");
            try
            {

                // Check if RoleWiseData is provided
                if (input.RoleWiseData == null || input.RoleWiseData.Count <= 0)
                    throw new ServiceException(Resources.Resource.ATLEAST_ONE_ROLE_REQUIRED);

                // Check if any penalty is missing a deadline
                if (input.Penalties != null && input.Penalties.Any(penalty => string.IsNullOrWhiteSpace(penalty.Deadline)))
                {
                    throw new ServiceException(Resources.Resource.DEADLINE_REQUIRED_FOR_PENALTY);
                }


                string accessibilityInfoString = (input.AccessibilityInfo != null && input.AccessibilityInfo.Any()) ? $"[{string.Join(",", input.AccessibilityInfo)}]" : null;

                string AccommodationPackageInfo = (input.AccommodationPackageInfo != null && input.AccommodationPackageInfo.Any()) ? $"[{string.Join(",", input.AccommodationPackageInfo)}]" : null;

                var roleWiseData = input.RoleWiseData != null ? JsonConvert.SerializeObject(input.RoleWiseData) : null;

                var paymentMethodSupported = input.PaymentMethodSupported != null ? JsonConvert.SerializeObject(input.PaymentMethodSupported) : null;


                if (id.HasValue)
                    objCmd.Parameters.AddWithValue("@Id", id);

                objCmd.Parameters.AddWithValue("@OrganizationId", organizationId);
                objCmd.Parameters.AddWithValue("@Name", input.Name);
                objCmd.Parameters.AddWithValue("@BannerImage", input.BannerImage);
                objCmd.Parameters.AddWithValue("@Description", input.Description);
                objCmd.Parameters.AddWithValue("@Latitude", input.Latitude);
                objCmd.Parameters.AddWithValue("@Longitude", input.Longitude);
                objCmd.Parameters.AddWithValue("@Address", input.Address);
                objCmd.Parameters.AddWithValue("@City", input.City);
                objCmd.Parameters.AddWithValue("@State", input.State);
                objCmd.Parameters.AddWithValue("@CountryId", input.CountryId);
                objCmd.Parameters.AddWithValue("@TimeZoneId", input.TimeZoneId);
                objCmd.Parameters.AddWithValue("@StartDate", input.StartDate);
                objCmd.Parameters.AddWithValue("@EndDate", input.EndDate);
                objCmd.Parameters.AddWithValue("@AccommodationInfoFile", input.AccommodationInfoFile);
                objCmd.Parameters.AddWithValue("@TransportationInfoFile", input.TransportationInfoFile);
                objCmd.Parameters.AddWithValue("@AccessibilityInfo", accessibilityInfoString);
                objCmd.Parameters.AddWithValue("@AccommodationPackageInfo", AccommodationPackageInfo);
                objCmd.Parameters.AddWithValue("@RoleWiseData", roleWiseData);
                objCmd.Parameters.AddWithValue("@PaymentMethodSupported", paymentMethodSupported);
                objCmd.Parameters.AddWithValue("@PaymentProviderId", input.PaymentProviderId);
                objCmd.Parameters.AddWithValue("@Status", StatusExtensions.ToStatusEnum(input.Status.ToLower()));
                objCmd.Parameters.AddWithValue("@Penalties", MapDataTable.ToDataTable(input.Penalties));

                // Create a DataTable to store hotel ids
                DataTable dtHotelIds = new DataTable();
                dtHotelIds.Columns.Add("HotelId", typeof(int));
                foreach (var hotelId in input.HotelIds)
                {
                    dtHotelIds.Rows.Add(hotelId);
                }
                objCmd.Parameters.AddWithValue("@HotelIds", dtHotelIds);

                DataTable dtEvent = await objSQL.FetchDT(objCmd);

                var error = Convert.ToInt64(dtEvent.Rows[0]["ErrorCode"]);
                var errorMessage = CommonUtilities.GetErrorMessage(error);
                if (!string.IsNullOrEmpty(errorMessage))
                    throw new ServiceException(errorMessage);

                return Convert.ToInt64(dtEvent.Rows[0]["OrganizationEventId"]);              
              
            }            
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (objSQL != null) objSQL.Dispose();
                if (objCmd != null) objCmd.Dispose();
            }
        }

        public async Task<EventListDto> GetAllEvents(long organizationId, string sortColumn, string sortOrder, string searchText, int? pageNo, int? pageSize)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetAllEvents");

            try
            {
                objCmd.Parameters.AddWithValue("@OrganizationId", organizationId);
                objCmd.Parameters.AddWithValue("@PageNumber", pageNo);
                objCmd.Parameters.AddWithValue("@PageSize", pageSize);
                objCmd.Parameters.AddWithValue("@SearchText", searchText);
                objCmd.Parameters.AddWithValue("@SortOrder", sortOrder);
                objCmd.Parameters.AddWithValue("@SortColumn", sortColumn);

                DataTable dt = await objSQL.FetchDT(objCmd);
                var events = new List<EventInfoDto>();
                var totalCount = 0;
                var totalRecords = 0;

                if (dt.Rows.Count > 0)
                {
                    events = (from DataRow dr in dt.Rows
                                select new EventInfoDto
                                {
                                        Id = Convert.ToInt64(dr["Id"]),
                                        OrganizationId = Convert.ToInt64(dr["OrganizationId"]),
                                        Name = Convert.ToString(dr["Name"]),
                                        BannerImage = Convert.ToString(dr["BannerImage"]),
                                        Description = Convert.ToString(dr["Description"]),
                                        Latitude = Convert.ToString(dr["Latitude"]),
                                        Longitude = Convert.ToString(dr["Longitude"]),
                                        Address = Convert.ToString(dr["Address"]),
                                        City = Convert.ToString(dr["City"]),
                                        State = Convert.ToString(dr["State"]),
                                        Country = Convert.ToString(dr["Country"]),
                                        CountryId = Convert.ToInt32(dr["CountryId"]),
                                        TimeZoneId = Convert.ToInt32(dr["TimeZoneId"]),
                                        StartDate = Convert.ToString(dr["StartDate"]),
                                        EndDate = Convert.ToString(dr["EndDate"]),
                                        AccommodationInfoFile = dr["AccommodationInfoFile"].ToString(),
                                        TransportationInfoFile = dr["TransportationInfoFile"].ToString(),
                                        Status = StatusExtensions.ToStatusString((Status)Convert.ToInt32(dr["Status"])),
                                        CreatedDate = Convert.ToDateTime(dr["CreatedDate"]),
                                        UpdatedDate = dr["UpdatedDate"] != DBNull.Value ? Convert.ToDateTime(dr["UpdatedDate"]) : (DateTime?)null,
                                        AccommodationPackageInfo = !string.IsNullOrEmpty(Convert.ToString(dr["AccommodationPackageInfo"])) ? ConversionHelper.ConvertStringToList(Convert.ToString(dr["AccommodationPackageInfo"])) : new List<string>(),
                                        AccessibilityInfo = !string.IsNullOrEmpty(Convert.ToString(dr["AccessibilityInfo"])) ? ConversionHelper.ConvertStringToArray(Convert.ToString(dr["AccessibilityInfo"])) : new List<int>(),
                                        AccessibilityInfoData = !string.IsNullOrEmpty(Convert.ToString(dr["AccessibilityInfoData"])) ? JsonConvert.DeserializeObject<List<AccessiblityInfoDto>>(Convert.ToString(dr["AccessibilityInfoData"])) : new List<AccessiblityInfoDto>(),
                                        PaymentMethodSupported = JsonConvert.DeserializeObject<List<PaymentMethodSupported>>(dr["PaymentMethodSupported"].ToString()),
                                        RoleWiseData = JsonConvert.DeserializeObject<List<RoleWiseData>>(dr["RoleWiseData"].ToString()),
                                        PaymentproviderId = dr["PaymentproviderId"] != DBNull.Value ? Convert.ToInt32(dr["PaymentproviderId"]) : (int?)null    
                                }).ToList();

                    totalCount = Convert.ToInt32(dt.Rows[0]["TotalCount"]);
                    totalRecords = Convert.ToInt32(dt.Rows[0]["TotalRecords"]);
                }

                return new EventListDto { List = events, TotalCount = totalCount, TotalRecords = totalRecords };
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

        public async Task<EventDetailsDto> GetById(long id, bool bankDetails = false)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetEventById");

            try
            {
                objCmd.Parameters.AddWithValue("@Id", id);
                objCmd.Parameters.AddWithValue("@BankDetails", bankDetails);

                DataSet ds = await objSQL.FetchDB(objCmd);


                EventDetailsDto eventDto = null;
                var hotels = new List<HotelDto>();
                var penalties = new List<Penalties>();
                var roomTypes = new List<HotelRoomType>();

                if (ds.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        eventDto = await EventMappings.MapToDto(dr);
                    }
                }

                if (ds.Tables[3].Rows.Count > 0)
                {
                    roomTypes = (from DataRow dr in ds.Tables[3].Rows
                                 select new HotelRoomType
                                 {
                                     Id = Convert.ToInt64(dr["Id"]),
                                     HotelId = Convert.ToInt64(dr["HotelId"]),
                                     RoomSize = Convert.ToString(dr["RoomSize"]),
                                     PackagePrice = Convert.ToDecimal(dr["PackagePrice"]),
                                     CurrencyId = Convert.ToInt32(dr["CurrencyId"]),
                                     NightPrice = Convert.ToDecimal(dr["NightPrice"]),
                                     Availability = Convert.ToInt32(dr["Availability"]),
                                     Status = StatusExtensions.ToStatusString((Status)Convert.ToInt32(dr["Status"])),
                                     MinimumOccupancy = dr["MinimumOccupancy"] != DBNull.Value ? Convert.ToInt32(dr["MinimumOccupancy"]) : (int?)null,
                                     MaximumOccupancy = dr["MaximumOccupancy"] != DBNull.Value ? Convert.ToInt32(dr["MaximumOccupancy"]) : (int?)null
                                 }).ToList();

                }

                if (ds.Tables[1].Rows.Count > 0)
                {
                    hotels = (from DataRow dr in ds.Tables[1].Rows
                              select new HotelDto
                              {
                                  Id = Convert.ToInt64(dr["HotelId"]),
                                  OrganizationId = Convert.ToInt64(dr["OrganizationId"]),
                                  Name = Convert.ToString(dr["Name"]),
                                  Rating = Convert.ToDouble(dr["Rating"]),
                                  Address = Convert.ToString(dr["Address"]),
                                  PostalCode = Convert.ToString(dr["PostalCode"]),
                                  City = Convert.ToString(dr["City"]),
                                  State = Convert.ToString(dr["State"]),
                                  Country = Convert.ToString(dr["Country"]),
                                  LocationLatLong = Convert.ToString(dr["LocationLatLong"]),
                                  CreatedDate = Convert.ToDateTime(dr["CreatedDate"]),
                                  UpdatedDate = dr["UpdatedDate"] != DBNull.Value ? Convert.ToDateTime(dr["UpdatedDate"]) : (DateTime?)null,
                                  RoomType = roomTypes.Where(e => e.HotelId == Convert.ToInt64(dr["HotelId"])).ToList()
                              }).ToList();
                }

                if (ds.Tables[2].Rows.Count > 0)
                {
                    penalties = (from DataRow dr in ds.Tables[2].Rows
                                 select new Penalties
                                 {
                                     PenaltyType = Convert.ToInt32(dr["PenaltyType"]),
                                     Deadline = Convert.ToString(dr["Deadline"]),
                                     Fees = Convert.ToDecimal(dr["Fees"]),
                                     CurrencyId = Convert.ToInt32(dr["CurrencyId"]),
                                     IsPercentage = Convert.ToBoolean(dr["IsPercentage"]),
                                 }).ToList();
                }

                if (eventDto != null)
                {
                    eventDto.Hotels = hotels;
                    eventDto.Penalties = penalties;
                }

                Assertions.IsNotNull(eventDto, Resources.Resource.DATABASE_ERROR_1003);

                return eventDto;
            }
            catch (Exception ex)
            {
                // Handle other exceptions as needed
                throw;
            }
            finally
            {
                objSQL?.Dispose();
                objCmd?.Dispose();
            }
        }

        public async Task<EventDetailsDtov1> GetEventById(long id, bool bankDetails = false)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetEventById_v1");

            try
            {
                objCmd.Parameters.AddWithValue("@Id", id);
                objCmd.Parameters.AddWithValue("@BankDetails", bankDetails);

                DataSet ds = await objSQL.FetchDB(objCmd);


                EventDetailsDtov1 eventDto = null;
                var penalties = new List<Penalties>();

                if (ds.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        eventDto = await EventMappings.MapToDto<EventDetailsDtov1>(dr);
                    }
                }

                if (ds.Tables[1].Rows.Count > 0)
                {
                    penalties = (from DataRow dr in ds.Tables[2].Rows
                                 select new Penalties
                                 {
                                     PenaltyType = Convert.ToInt32(dr["PenaltyType"]),
                                     Deadline = Convert.ToString(dr["Deadline"]),
                                     Fees = Convert.ToDecimal(dr["Fees"]),
                                     CurrencyId = Convert.ToInt32(dr["CurrencyId"]),
                                     IsPercentage = Convert.ToBoolean(dr["IsPercentage"]),
                                 }).ToList();
                }

                if (eventDto != null)
                    eventDto.Penalties = penalties;

                Assertions.IsNotNull(eventDto, Resources.Resource.DATABASE_ERROR_1003);

                return eventDto;
            }
            catch (Exception ex)
            {
                // Handle other exceptions as needed
                throw;
            }
            finally
            {
                objSQL?.Dispose();
                objCmd?.Dispose();
            }
        }


        public async Task<List<Penalties>> GetPenaltyByEventId(long OrganizationEventId)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetPenaltyByEventId");

            try
            {
                objCmd.Parameters.AddWithValue("@EventId", OrganizationEventId);
                DataTable dt = await objSQL.FetchDT(objCmd);

                var events = (from DataRow dr in dt.Rows
                                    select new Penalties
                                    {
                                        PenaltyType = Convert.ToInt32(dr["PenaltyType"]),
                                        Deadline = Convert.ToString(dr["Deadline"]),
                                        Fees = Convert.ToDecimal(dr["Fees"]),
                                        CurrencyId = Convert.ToInt32(dr["CurrencyId"]),
                                        IsPercentage = Convert.ToBoolean(dr["IsPercentage"]),
                                    }).ToList();

                return events;
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

        public async Task<long> DeleteEvent(long id)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_DeleteEvent");

            try
            {
                objCmd.CommandType = CommandType.StoredProcedure;
                objCmd.Parameters.AddWithValue("@EventId", id);
                DataTable dtEvent = await objSQL.FetchDT(objCmd);

                var error = Convert.ToInt64(dtEvent.Rows[0]["ErrorCode"]);
                var errorMessage = CommonUtilities.GetErrorMessage(error);
                if (!string.IsNullOrEmpty(errorMessage))
                    throw new ServiceException(errorMessage);

                return id;
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

        public async Task<EventListDto> GetEventsByOrganizationId(long organizationId, string sortColumn, string sortOrder, string searchText, int? pageNo, int? pageSize, string status = null)
        {
            
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetEventsByOrganizationId");

            try
            {
                objCmd.Parameters.AddWithValue("@OrganizationId", organizationId);
                objCmd.Parameters.AddWithValue("@PageNumber", pageNo);
                objCmd.Parameters.AddWithValue("@PageSize", pageSize);
                objCmd.Parameters.AddWithValue("@SearchText", searchText);
                objCmd.Parameters.AddWithValue("@SortOrder", sortOrder);
                objCmd.Parameters.AddWithValue("@SortColumn", sortColumn);
                objCmd.Parameters.AddWithValue("@Status", status != null ? StatusExtensions.ToStatusEnum(status.ToLower()) : null);

                DataTable dt = await objSQL.FetchDT(objCmd);
                List<EventInfoDto> events = new List<EventInfoDto>();
                var totalCount = 0;

                if (dt.Rows.Count > 0)
                {
                    events = (from DataRow dr in dt.Rows
                              select new EventInfoDto
                              {
                                  Id = Convert.ToInt64(dr["Id"]),
                                  OrganizationId = Convert.ToInt64(dr["OrganizationId"]),
                                  Name = Convert.ToString(dr["Name"]),
                                  BannerImage = Convert.ToString(dr["BannerImage"]),
                                  Description = Convert.ToString(dr["Description"]),
                                  Latitude = Convert.ToString(dr["Latitude"]),
                                  Longitude = Convert.ToString(dr["Longitude"]),
                                  Address = Convert.ToString(dr["Address"]),
                                  City = Convert.ToString(dr["City"]),
                                  State = Convert.ToString(dr["State"]),
                                  Country = Convert.ToString(dr["Country"]),
                                  CountryId = Convert.ToInt32(dr["CountryId"]),
                                  TimeZoneId = Convert.ToInt32(dr["TimeZoneId"]),
                                  StartDate = Convert.ToString(dr["StartDate"]),
                                  EndDate = Convert.ToString(dr["EndDate"]),
                                  AccommodationInfoFile = dr["AccommodationInfoFile"].ToString(),
                                  TransportationInfoFile = dr["TransportationInfoFile"].ToString(),
                                  Status = StatusExtensions.ToStatusString((Status)Convert.ToInt32(dr["Status"])),
                                  CreatedDate = Convert.ToDateTime(dr["CreatedDate"]),
                                  UpdatedDate = dr["UpdatedDate"] != DBNull.Value ? Convert.ToDateTime(dr["UpdatedDate"]) : (DateTime?)null,
                                  AccommodationPackageInfo = !string.IsNullOrEmpty(Convert.ToString(dr["AccommodationPackageInfo"])) ? ConversionHelper.ConvertStringToList(Convert.ToString(dr["AccommodationPackageInfo"])) : new List<string>(),
                                  AccessibilityInfo = !string.IsNullOrEmpty(Convert.ToString(dr["AccessibilityInfo"])) ? ConversionHelper.ConvertStringToArray(Convert.ToString(dr["AccessibilityInfo"])) : new List<int>(),
                                  AccessibilityInfoData = !string.IsNullOrEmpty(Convert.ToString(dr["AccessibilityInfoData"])) ? JsonConvert.DeserializeObject<List<AccessiblityInfoDto>>(Convert.ToString(dr["AccessibilityInfoData"])) : new List<AccessiblityInfoDto>(),
                                  PaymentMethodSupported = JsonConvert.DeserializeObject<List<PaymentMethodSupported>>(dr["PaymentMethodSupported"].ToString()),
                                  RoleWiseData = JsonConvert.DeserializeObject<List<RoleWiseData>>(dr["RoleWiseData"].ToString()),
                                  PaymentproviderId = dr["PaymentproviderId"] != DBNull.Value ? Convert.ToInt32(dr["PaymentproviderId"]) : (int?)null
                              }).ToList();

                    totalCount = Convert.ToInt32(dt.Rows[0]["TotalCount"]);
                }
                return new EventListDto { List = events , TotalCount = totalCount };
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

        public async Task<bool> ContactUs(long organizationId, ContactUs input)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_ContactUs");
            try
            {
                
                objCmd.Parameters.AddWithValue("@OrganizationId", organizationId);
                objCmd.Parameters.AddWithValue("@Name", input.Name);
                objCmd.Parameters.AddWithValue("@Email", input.Email);
                objCmd.Parameters.AddWithValue("@Message", input.Message);

                DataTable dtEvent = await objSQL.FetchDT(objCmd);

                return true;
              
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (objSQL != null) objSQL.Dispose();
                if (objCmd != null) objCmd.Dispose();
            }
        }

        public async Task<long> AddAnnouncement(long? id, long organizationId, Announcement input)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_AddOrUpdateAnnouncement");
            try
            {

                if (id.HasValue)
                    objCmd.Parameters.AddWithValue("@Id", id);

                objCmd.Parameters.AddWithValue("@OrganizationId", organizationId);
                objCmd.Parameters.AddWithValue("@Heading", input.Heading);
                objCmd.Parameters.AddWithValue("@Image", input.Image);
                objCmd.Parameters.AddWithValue("@Description", input.Description);
                objCmd.Parameters.AddWithValue("@Location", input.Location);
                objCmd.Parameters.AddWithValue("@StartDate", input.StartDate);
                objCmd.Parameters.AddWithValue("@EndDate", input.EndDate);
                objCmd.Parameters.AddWithValue("@Status", StatusExtensions.ToStatusEnum(input.Status.ToLower()));
               
                DataTable dtEvent = await objSQL.FetchDT(objCmd);

                //var error = Convert.ToInt64(dtEvent.Rows[0]["ErrorCode"]);
                //var errorMessage = CommonUtilities.GetErrorMessage(error);
                //if (!string.IsNullOrEmpty(errorMessage))
                //    throw new ServiceException(errorMessage);

                return Convert.ToInt64(dtEvent.Rows[0]["Id"]);

            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (objSQL != null) objSQL.Dispose();
                if (objCmd != null) objCmd.Dispose();
            }
        }

        public async Task<long> DeleteAnnouncement(long id)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_DeleteAnnouncement");

            try
            {
                objCmd.CommandType = CommandType.StoredProcedure;
                objCmd.Parameters.AddWithValue("@Id", id);
                DataTable dtEvent = await objSQL.FetchDT(objCmd);

                var error = Convert.ToInt64(dtEvent.Rows[0]["ErrorCode"]);
                var errorMessage = CommonUtilities.GetErrorMessage(error);
                if (!string.IsNullOrEmpty(errorMessage))
                    throw new ServiceException(errorMessage);

                return id;
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

        public async Task<AnnouncementListDto> GetAllAnnouncements(long organizationId, string sortColumn, string sortOrder, int? pageNo, int? pageSize)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetAnnouncementList");

            try
            {
                objCmd.Parameters.AddWithValue("@OrganizationId", organizationId);
                objCmd.Parameters.AddWithValue("@PageNumber", pageNo);
                objCmd.Parameters.AddWithValue("@PageSize", pageSize);
                objCmd.Parameters.AddWithValue("@SortOrder", sortOrder);
                objCmd.Parameters.AddWithValue("@SortColumn", sortColumn);

                DataTable dt = await objSQL.FetchDT(objCmd);
                var announcements = new List<AnnouncementDto>();
                var totalCount = 0;

                if (dt.Rows.Count > 0)
                {
                    announcements = (from DataRow dr in dt.Rows
                              select new AnnouncementDto
                              {
                                  Id = Convert.ToInt64(dr["Id"]),
                                  OrganizationId = Convert.ToInt64(dr["OrganizationId"]),
                                  Heading = Convert.ToString(dr["Heading"]),
                                  Image = Convert.ToString(dr["Image"]),
                                  Description = Convert.ToString(dr["Description"]),
                                  Location = Convert.ToString(dr["Location"]),
                                  StartDate = dr["StartDate"] != DBNull.Value ? DateTimeOffset.Parse(dr["StartDate"].ToString()) : (DateTimeOffset?)null,
                                  EndDate = dr["EndDate"] != DBNull.Value ? DateTimeOffset.Parse(dr["EndDate"].ToString()) : (DateTimeOffset?)null,
                                  Status = StatusExtensions.ToStatusString((Status)Convert.ToInt32(dr["Status"])),
                                  CreatedDate = Convert.ToDateTime(dr["CreatedDate"]),
                                  UpdatedDate = dr["UpdatedDate"] != DBNull.Value ? Convert.ToDateTime(dr["UpdatedDate"]) : (DateTime?)null
                              }).ToList();

                    totalCount = Convert.ToInt32(dt.Rows[0]["TotalCount"]);
                }

                return new AnnouncementListDto { List = announcements, TotalCount = totalCount };
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

        public async Task<List<EventDto>> GetEventsByOrganizationId(long organizationId)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetEventByOrganizationId");

            try
            {
                objCmd.Parameters.AddWithValue("@OrganizationId", organizationId);

                DataTable dt = await objSQL.FetchDT(objCmd);

                var events = (from DataRow dr in dt.Rows
                              select new EventDto
                              {
                                  Id = Convert.ToInt32(dr["Id"]),
                                  Name = Convert.ToString(dr["Name"])
                              }).ToList();

                return events;
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
