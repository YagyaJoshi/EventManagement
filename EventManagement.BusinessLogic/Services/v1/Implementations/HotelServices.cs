using EventManagement.BusinessLogic.Services.v1.Abstractions;
using EventManagement.DataAccess;
using EventManagement.DataAccess.Models;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Data;
using EventManagement.DataAccess.ViewModels.Dtos;
using EventManagement.BusinessLogic.Helpers;
using EventManagement.BusinessLogic.Exceptions;
using EventManagement.DataAccess.Enums;
using EventManagement.DataAccess.Extensions;
using EventManagement.DataAccess.ViewModels.ApiObjects;
using Microsoft.Extensions.Logging;


namespace EventManagement.BusinessLogic.Services.v1.Implementations
{
    public class HotelServices : IHotelServices
    {
        private readonly IConfiguration _configuration;

        public HotelServices(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<long> AddHotel(long? id,long organizationId, Hotel input)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_AddOrUpdateHotel");
            try
            {
                if (input.RoomType.Count <= 0)
                    throw new ServiceException(Resources.Resource.ATLEAST_ONE_ROOM_REQUIRED);

                if (id.HasValue)
                    objCmd.Parameters.AddWithValue("@Id", id);
                objCmd.Parameters.AddWithValue("@OrganizationId", organizationId);
                objCmd.Parameters.AddWithValue("@Name", input.Name);
                objCmd.Parameters.AddWithValue("@Rating", input.Rating);
                objCmd.Parameters.AddWithValue("@Address", input.Address);
                objCmd.Parameters.AddWithValue("@PostalCode", input.PostalCode);
                objCmd.Parameters.AddWithValue("@City", input.City);
                objCmd.Parameters.AddWithValue("@State", input.State);
                objCmd.Parameters.AddWithValue("@CountryId", input.CountryId);
                objCmd.Parameters.AddWithValue("@LocationLatLong", input.LocationLatLong);
                //objCmd.Parameters.AddWithValue("@Status", StatusExtensions.ToStatusEnum(input.Status.ToLower()));

                 objCmd.Parameters.AddWithValue("@RoomType", MapDataTable.ToDataTable(input.RoomType));

                DataTable dtEventIds = new DataTable();
                dtEventIds.Columns.Add("OrganizationEventId", typeof(int));
                foreach (var eventId in input.EventIds)
                {
                    dtEventIds.Rows.Add(eventId);
                }
                objCmd.Parameters.AddWithValue("@EventIds", dtEventIds);

                DataTable dtHotel = await objSQL.FetchDT(objCmd);

                var error = Convert.ToInt64(dtHotel.Rows[0]["ErrorCode"]);
                var errorMessage = CommonUtilities.GetErrorMessage(error);
                if (!string.IsNullOrEmpty(errorMessage))
                    throw new ServiceException(errorMessage);

                return Convert.ToInt64(dtHotel.Rows[0]["HotelId"]);
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


        public async Task<HotelListDto> GetAllHotels(long organizationId, string sortColumn, string sortOrder, string searchText, int? pageNo, int? pageSize)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetAllHotels");

            try
            {
                objCmd.Parameters.AddWithValue("@OrganizationId", organizationId);
                objCmd.Parameters.AddWithValue("@PageNumber", pageNo);
                objCmd.Parameters.AddWithValue("@PageSize", pageSize);
                objCmd.Parameters.AddWithValue("@SearchText", searchText);
                objCmd.Parameters.AddWithValue("@SortOrder", sortOrder);
                objCmd.Parameters.AddWithValue("@SortColumn", sortColumn);

                DataSet ds = await objSQL.FetchDB(objCmd);
                List<HotelDto> hotels = new List<HotelDto>();
                var rooms = new List<HotelRoomType>();
                Dictionary<long, List<int>> hotelEvents = new Dictionary<long, List<int>>();
                int totalCount = 0;

                if (ds.Tables[1].Rows.Count > 0)
                {
                    rooms = (from DataRow dr in ds.Tables[1].Rows
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

                // Fetch Assigned Events to Hotels
                if (ds.Tables[2].Rows.Count > 0)
                {
                    foreach (DataRow dr in ds.Tables[2].Rows)
                    {
                        long hotelId = Convert.ToInt64(dr["HotelId"]);
                        int eventId = Convert.ToInt32(dr["OrganizationEventId"]);

                        if (!hotelEvents.ContainsKey(hotelId))
                        {
                            hotelEvents[hotelId] = new List<int>();
                        }

                        hotelEvents[hotelId].Add(eventId);
                    }
                }

                if (ds.Tables[0].Rows.Count > 0)
                {
                    hotels = (from DataRow dr in ds.Tables[0].Rows
                              select new HotelDto
                              {
                                  Id = Convert.ToInt64(dr["Id"]),
                                  OrganizationId = Convert.ToInt64(dr["OrganizationId"]),
                                  Name = Convert.ToString(dr["Name"]),
                                  Rating = Convert.ToDouble(dr["Rating"]),
                                  Address = Convert.ToString(dr["Address"]),
                                  PostalCode = Convert.ToString(dr["PostalCode"]),
                                  City = Convert.ToString(dr["City"]),
                                  State = Convert.ToString(dr["State"]),
                                  Country = Convert.ToString(dr["Country"]),
                                  CountryId = dr["CountryId"] != DBNull.Value ? Convert.ToInt32(dr["CountryId"]) : null,
                                  LocationLatLong = Convert.ToString(dr["LocationLatLong"]),
                                  CreatedDate = Convert.ToDateTime(dr["CreatedDate"]),
                                  UpdatedDate = dr["UpdatedDate"] != DBNull.Value ? Convert.ToDateTime(dr["UpdatedDate"]) : (DateTime?)null,
                                  RoomType = rooms.Where(e => e.HotelId == Convert.ToInt64(dr["Id"])).ToList(),
                                  EventIds = hotelEvents.ContainsKey(Convert.ToInt64(dr["Id"])) ? hotelEvents[Convert.ToInt64(dr["Id"])] : new List<int>()
                              }).ToList();


                    totalCount = Convert.ToInt32(ds.Tables[0].Rows[0]["TotalCount"]);

                }

                return new HotelListDto { List = hotels, TotalCount = totalCount };
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


        public async Task<HotelDto> GetHotelById(long id)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetHotelById");

            try
            {
                HotelDto hotel = null;
                objCmd.Parameters.AddWithValue("@Id", id);

                DataSet ds = await objSQL.FetchDB(objCmd);

                if (ds.Tables[0].Rows.Count > 0)
                {
                    hotel = (from DataRow dr in ds.Tables[0].Rows
                                 select new HotelDto
                                 {
                                     Id = Convert.ToInt64(dr["Id"]),
                                     OrganizationId = Convert.ToInt64(dr["OrganizationId"]),
                                     Name = Convert.ToString(dr["Name"]),
                                     Rating = Convert.ToDouble(dr["Rating"]),
                                     Address = Convert.ToString(dr["Address"]),
                                     PostalCode = Convert.ToString(dr["PostalCode"]),
                                     City = Convert.ToString(dr["City"]),
                                     State = Convert.ToString(dr["State"]),
                                     Country = Convert.ToString(dr["Country"]),
                                     CountryId = dr["CountryId"] != DBNull.Value ? Convert.ToInt32(dr["CountryId"]) : null,
                                     LocationLatLong = Convert.ToString(dr["LocationLatLong"]),     
                                     //Status = StatusExtensions.ToStatusString((Status)Convert.ToInt32(dr["Status"])),
                                     CreatedDate = Convert.ToDateTime(dr["CreatedDate"]),
                                     UpdatedDate = dr["UpdatedDate"] != DBNull.Value ? Convert.ToDateTime(dr["UpdatedDate"]) : (DateTime?)null
                                 }).FirstOrDefault();

                    if (ds.Tables[1].Rows.Count > 0)
                    {
                        var rooms = (from DataRow dr in ds.Tables[1].Rows
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

                        hotel.RoomType = rooms;
                    }

                    if (ds.Tables[2].Rows.Count > 0)
                    {
                        var eventIds = (from DataRow dr in ds.Tables[2].Rows
                                        select Convert.ToInt32(dr["OrganizationEventId"])).ToList();

                        hotel.EventIds = eventIds;
                    }

                    return hotel;
                }

                Assertions.IsNotNull(hotel, Resources.Resource.DATABASE_ERROR_1012);

                return null;
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

        public async Task<long> DeleteHotel(long id)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_DeleteHotel");

            try
            {
                objCmd.CommandType = CommandType.StoredProcedure;
                objCmd.Parameters.AddWithValue("Id", id);
                DataTable dtHotel = await objSQL.FetchDT(objCmd);

                var error = Convert.ToInt64(dtHotel.Rows[0]["ErrorCode"]);
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

        public async Task<List<HotelRoomType>> GetHotelRoomTypes(long hotelId)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetHotelRoomTypes");

            try
            {
                objCmd.Parameters.AddWithValue("@HotelId", hotelId);
                DataTable dt = await objSQL.FetchDT(objCmd);

                var roomTypes = (from DataRow dr in dt.Rows
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

                return roomTypes;
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

        public async Task<List<int>> GetEventIds(long hotelId)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetEventIds");

            try
            {
                objCmd.Parameters.AddWithValue("@HotelId", hotelId);
                DataTable dt = await objSQL.FetchDT(objCmd);

                var eventIds = (from DataRow dr in dt.Rows
                                select Convert.ToInt32(dr["OrganizationEventId"])).ToList();

                return eventIds;
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

        public async Task<int?> CheckRoomAvailability(RoomAvailabilityInput input)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_CheckRoomAvailability");

            try
            {
                int? availableRooms = 0;

                // Add parameters
                objCmd.Parameters.AddWithValue("@EventId", input.EventId);
                objCmd.Parameters.AddWithValue("@HotelId", input.HotelId);
                objCmd.Parameters.AddWithValue("@RoomTypeId", input.RoomTypeId);

                // Execute the stored procedure and fetch the result set
                DataSet ds = await objSQL.FetchDB(objCmd);

                if (ds.Tables[0].Rows.Count > 0)
                {
                    availableRooms = ds.Tables[0].Rows[0]["AvailableRooms"] != DBNull.Value
                            ? Convert.ToInt32(ds.Tables[0].Rows[0]["AvailableRooms"])
                            : (int?)null;
                }

                // Ensure the value is not null
                //Assertions.IsNotNull(availableRooms, Resources.Resource.DATABASE_ERROR_1012);

                return availableRooms;
            }
            catch (Exception ex)
            {
                throw; // Rethrow the exception
            }
            finally
            {
                // Dispose of SQLManager and SqlCommand objects
                if (objSQL != null) objSQL.Dispose();
                if (objCmd != null) objCmd.Dispose();
            }
        }

        public async Task<List<HotelInfoDto>> GetHotelsByOrganizationId(long organizationId)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetHotelsByOrganizationId");

            try
            {
                objCmd.Parameters.AddWithValue("@OrganizationId", organizationId);

                DataTable dt = await objSQL.FetchDT(objCmd);

                var hotels = (from DataRow dr in dt.Rows
                              select new HotelInfoDto
                              {
                                  Id = Convert.ToInt32(dr["Id"]),
                                  Name = Convert.ToString(dr["Name"])
                              }).ToList();

                return hotels;
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
