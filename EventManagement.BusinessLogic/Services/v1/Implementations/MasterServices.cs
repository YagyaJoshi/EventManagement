using EventManagement.BusinessLogic.Services.v1.Abstractions;
using EventManagement.DataAccess;
using EventManagement.DataAccess.ViewModels.Dtos;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Data;
using EventManagement.DataAccess.Models;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;
using System;


namespace EventManagement.BusinessLogic.Services.v1.Implementations
{
    public class MasterServices : IMasterServices
    {
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _memoryCache;

        public MasterServices(IConfiguration configuration, IMemoryCache memoryCache)
        {
            _configuration = configuration;
            _memoryCache = memoryCache;
        }


        public async Task<List<TimeZoneDto>> GetTimeZones()
        {
            const string cacheKey = "TimeZones";
            if (!_memoryCache.TryGetValue(cacheKey, out List<TimeZoneDto> timeZones))
            {
                SQLManager objSQL = new SQLManager(_configuration);
                SqlCommand objCmd = new SqlCommand("sp_GetTimeZone");

                try
                {
                    DataTable dtZones = await objSQL.FetchDT(objCmd);

                    timeZones = (from DataRow dr in dtZones.Rows
                                 select new TimeZoneDto
                                 {
                                     Id = Convert.ToInt64(dr["Id"]),
                                     Text = Convert.ToString(dr["Text"])
                                 }).ToList();

                    // Set cache options.
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromDays(365)); 

                    // Save data in cache.
                    _memoryCache.Set(cacheKey, timeZones, cacheEntryOptions);
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    objSQL?.Dispose();
                    objCmd?.Dispose();
                }
            }
            return timeZones;
        }


        public async Task<List<AccessiblityInfoDto>> GetAccessiblities()
        {
            const string cacheKey = "Accessibilities";
            if (!_memoryCache.TryGetValue(cacheKey, out List<AccessiblityInfoDto> accessibilities))
            {
                SQLManager objSQL = new SQLManager(_configuration);
                SqlCommand objCmd = new SqlCommand("sp_GetAccessiblities");
                try
                {
                    DataTable dt = await objSQL.FetchDT(objCmd);

                    accessibilities = (from DataRow dr in dt.Rows
                                          select new AccessiblityInfoDto
                                          {
                                              Id = Convert.ToInt32(dr["Id"]),
                                              Name = Convert.ToString(dr["Name"]),
                                              ImageURL = Convert.ToString(dr["ImageURL"])
                                          }).ToList();
                    // Set cache options.
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromDays(365));

                    // Save data in cache.
                    _memoryCache.Set(cacheKey, accessibilities, cacheEntryOptions);
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
            return accessibilities;
        }

        public async Task<List<CurrencyDto>> GetCurrencies()
        {
            const string cacheKey = "Currencies";
            if (!_memoryCache.TryGetValue(cacheKey, out List<CurrencyDto> currencies))
            {
                SQLManager objSQL = new SQLManager(_configuration);
                SqlCommand objCmd = new SqlCommand("sp_GetCurrencies");
                try
                {
                    DataTable dt = await objSQL.FetchDT(objCmd);

                     currencies = (from DataRow dr in dt.Rows
                                      select new CurrencyDto
                                      {
                                          Id = Convert.ToInt32(dr["Id"]),
                                          Name = Convert.ToString(dr["Name"]),
                                          Code = Convert.ToString(dr["Code"]),
                                          Symbol = Convert.ToString(dr["Symbol"])
                                      }).ToList();

                    // Set cache options.
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromDays(365));

                    // Save data in cache.
                    _memoryCache.Set(cacheKey, currencies, cacheEntryOptions);

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

            return currencies;
        }

        //public async Task<Dictionary<int, object>> GetRoleWiseModules(int roleId)
        //{
        //    SQLManager objSQL = new SQLManager(_configuration);
        //    SqlCommand objCmd = new SqlCommand("sp_GetRoleWiseModules");
        //    try
        //    {
        //        objCmd.Parameters.AddWithValue("@RoleId", roleId);
        //        DataTable dt = await objSQL.FetchDT(objCmd);

        //        var modules = (from DataRow dr in dt.Rows
        //                          select new Modules
        //                          {
        //                              Id = Convert.ToInt32(dr["Id"]),
        //                              Name = Convert.ToString(dr["Name"]),   
        //                              Add = Convert.ToBoolean(dr["IsAdd"]),
        //                              Delete = Convert.ToBoolean(dr["IsDelete"]),
        //                              Update = Convert.ToBoolean(dr["IsUpdate"]),
        //                              Visible = Convert.ToBoolean(dr["IsVisible"])
        //                          }).ToList();

        //        var transformedModules = new Dictionary<int, object>();

        //        foreach (var module in modules)
        //        {
        //            var id = module.Id;

        //            var moduleData = new
        //            {
        //                id = module.Id,
        //                name = module.Name,
        //                permissions = new
        //                {
        //                    add = module.Add,
        //                    edit = module.Update,
        //                    delete = module.Delete,
        //                    view = module.Visible,
        //                }
        //            };

        //            transformedModules[id] = moduleData;
        //        }
        //        return transformedModules;
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //    finally
        //    {
        //        if (objSQL != null) objSQL.Dispose();
        //        if (objCmd != null) objCmd.Dispose();
        //    }
        //}

        public async Task<List<object>> GetRoleWiseModules(int roleId)
        {
            using (SQLManager objSQL = new SQLManager(_configuration))
            using (SqlCommand objCmd = new SqlCommand("sp_GetRoleWiseModules"))
            {
                try
                {
                    objCmd.Parameters.AddWithValue("@RoleId", roleId);
                    DataTable dt = await objSQL.FetchDT(objCmd);

                    var modules = (from DataRow dr in dt.Rows
                                   select new Modules
                                   {
                                       Id = Convert.ToInt32(dr["Id"]),
                                       Name = Convert.ToString(dr["Name"]),
                                       Add = Convert.ToBoolean(dr["IsAdd"]),
                                       Delete = Convert.ToBoolean(dr["IsDelete"]),
                                       Update = Convert.ToBoolean(dr["IsUpdate"]),
                                       View = Convert.ToBoolean(dr["IsVisible"])
                                   }).ToList();

                    var transformedModules = modules.Select(module => new
                    {
                        id = module.Id,
                        name = module.Name,
                        permissions = new
                        {
                            add = module.Add,
                            edit = module.Update,
                            delete = module.Delete,
                            view = module.View,
                        }
                    }).Cast<object>().ToList();

                    return transformedModules;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        public async Task<List<CountriesMst>> GetCountries()
        {
            const string cacheKey = "Countries";
            if (!_memoryCache.TryGetValue(cacheKey, out List<CountriesMst> countries))
            {
                SQLManager objSQL = new SQLManager(_configuration);
                SqlCommand objCmd = new SqlCommand("sp_GetCountriesList");
                try
                {
                    DataTable dt = await objSQL.FetchDT(objCmd);

                    countries = (from DataRow dr in dt.Rows
                                     select new CountriesMst
                                     {
                                         Id = Convert.ToInt32(dr["Id"]),
                                         Name = Convert.ToString(dr["Name"]),
                                         Code = Convert.ToString(dr["Code"])
                                     }).ToList();

                    // Set cache options.
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromDays(365));

                    // Save data in cache.
                    _memoryCache.Set(cacheKey, countries, cacheEntryOptions);
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
            return countries;
        }

        public async Task<NotificationListDto> GetAllNotification(long organizationId, long userId,int? pageNo, int? pageSize, string sortColumn, string sortOrder, string userRole)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetAllNotification");

            try
            {
                objCmd.Parameters.AddWithValue("@OrganizationId", organizationId);
                // Optimized addition of UserId parameter
                objCmd.Parameters.AddWithValue("@UserRole", userRole);
                objCmd.Parameters.AddWithValue("@UserId", userRole == "customer" ? (object)userId : DBNull.Value);
                objCmd.Parameters.AddWithValue("@PageNumber", pageNo);
                objCmd.Parameters.AddWithValue("@PageSize", pageSize);
                objCmd.Parameters.AddWithValue("@SortOrder", sortOrder);
                objCmd.Parameters.AddWithValue("@SortColumn", sortColumn);

                DataTable dt = await objSQL.FetchDT(objCmd);
                var notification = new List<NotificationDto>();
                var totalCount = 0;

                if (dt.Rows.Count > 0)
                {
                    notification = (from DataRow dr in dt.Rows
                                    select new NotificationDto
                                    {
                                        Id = Convert.ToInt64(dr["Id"]),
                                        NotificationTypeId = Convert.ToInt32(dr["NotificationTypeId"]),
                                        OrganizationId = Convert.ToInt64(dr["OrganizationId"]),
                                        ActionType = Convert.ToInt32(dr["ActionType"]),
                                        UserId = dr["UserId"] != DBNull.Value ? Convert.ToInt64(dr["UserId"]) : 0,
                                        NotificationDateTime = Convert.ToString(dr["NotificationDateTime"]),
                                        MessageTitle = Convert.ToString(dr["MessageTitle"]),
                                        Message = Convert.ToString(dr["Message"]),
                                        IsRead = Convert.ToBoolean(dr["IsRead"]),
                                        CreatedDate = Convert.ToString(dr["CreatedDate"]),

                                    }).ToList();

                    totalCount = Convert.ToInt32(dt.Rows[0]["TotalCount"]);
                }

                return new NotificationListDto { List = notification, TotalCount = totalCount };
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
