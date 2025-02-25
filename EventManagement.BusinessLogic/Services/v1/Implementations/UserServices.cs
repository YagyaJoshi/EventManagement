using EventManagement.BusinessLogic.Services.v1.Abstractions;
using EventManagement.DataAccess;
using EventManagement.DataAccess.ViewModels.Dtos;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Data;
using EventManagement.DataAccess.ViewModels.ApiObjects;
using System.Text.Json;
using EventManagement.BusinessLogic.Resources;
using EventManagement.DataAccess.Enums;
using EventManagement.DataAccess.Extensions;
using EventManagement.BusinessLogic.Services.v1.Mappings;
using EventManagement.DataAccess.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Drawing.Printing;
using EventManagement.BusinessLogic.Exceptions;
using EventManagement.BusinessLogic.Helpers;
using EventManagement.Utilities.Helpers;

namespace EventManagement.BusinessLogic.Services.v1.Implementations
{
    public class UserServices : IUserServices
    {
        private readonly IConfiguration _configuration;

        private readonly IMasterServices _masterServices;

        public UserServices(IConfiguration configuration, IMasterServices masterServices)
        {
            _configuration = configuration;
            _masterServices = masterServices;
        }

        public async Task<UpdateUserInput> UpdateUser(long userId, string role, UpdateUserInput model)
        {
            if (role.ToLower() == EventManagement.DataAccess.Enums.Roles.Customer.ToString().ToLower() && (model.CustomerOrganizationTypeId == null || model.CustomerOrganizationTypeId <= 0 || string.IsNullOrEmpty(model.CustomerOrganizationName)))
                throw new ServiceException("CustomerOrganizationTypeId and CustomerOrganizationName are required");

            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_UpdateUserDetails");

            try
            {
                objCmd.CommandType = CommandType.StoredProcedure;
                objCmd.Parameters.AddWithValue("@Id", userId);
                objCmd.Parameters.AddWithValue("@FirstName", model.FirstName);
                objCmd.Parameters.AddWithValue("@LastName", model.LastName);
                objCmd.Parameters.AddWithValue("@Phone", model.Phone);
                objCmd.Parameters.AddWithValue("@CountryId", model.CountryId);
                if(model.CustomerOrganizationTypeId.HasValue)
                    objCmd.Parameters.AddWithValue("@OrganizationTypeId", model.CustomerOrganizationTypeId);
                objCmd.Parameters.AddWithValue("@OrganizationName", model.CustomerOrganizationName);

                await objSQL.UpdateDB(objCmd);
                return model;
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                objSQL.Dispose();
                objCmd.Dispose();
            }
        }

        //public async Task<UserProfileDto> GetUserById(long userId)
        //{
        //    SQLManager objSQL = new SQLManager(_configuration);
        //    SqlCommand objCmd = new SqlCommand("sp_GetUserById");

        //    UserProfileDto user = null;
        //    var modules = new List<Modules>();

        //    try
        //    {
        //        objCmd.Parameters.AddWithValue("@Id", userId);
        //        DataSet ds = await objSQL.FetchDB(objCmd);

        //        if (ds.Tables[0].Rows.Count > 0)
        //        {
        //            user = (from DataRow dr in ds.Tables[0].Rows
        //                    select new UserProfileDto
        //                    {
        //                        Id = Convert.ToInt64(dr["Id"]),
        //                        FirstName = dr["FirstName"].ToString(),
        //                        LastName = dr["LastName"].ToString(),
        //                        Phone = dr["Phone"].ToString(),
        //                        Email = dr["Email"].ToString(),
        //                        Country = dr["Country"].ToString(),
        //                        CountryId = dr["CountryId"] != DBNull.Value ? Convert.ToInt32(dr["CountryId"]) : null,
        //                        OrganizationId = dr["OrganizationId"] == DBNull.Value ? (long?)null : Convert.ToInt64(dr["OrganizationId"]),
        //                        Status = StatusExtensions.ToStatusString((Status)Convert.ToInt32(dr["Status"])),
        //                        CreatedDate = Convert.ToDateTime(dr["CreatedDate"]),
        //                        UpdatedDate = dr["UpdatedDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["UpdatedDate"]),
        //                        RoleId = Convert.ToInt32(dr["RoleId"]),
        //                        Role = dr["Role"].ToString(),
        //                        FcmToken = dr["FcmToken"].ToString(),
        //                        CustomerOrganization = dr["OrganizationTypeId"] != DBNull.Value ? new CustomerOrganization()
        //                        {
        //                            Id = Convert.ToInt32(dr["OrganizationTypeId"]),
        //                            Type = dr["OrganizationType"].ToString(),
        //                            Name = dr["OrganizationName"].ToString(),
        //                        } : null,
        //                    }).FirstOrDefault();

        //            if (ds.Tables[1].Rows.Count > 0)
        //            {
        //                modules = (from DataRow dr in ds.Tables[1].Rows
        //                           select new Modules
        //                           {
        //                               Id = Convert.ToInt32(dr["Id"]),
        //                               Name = Convert.ToString(dr["Name"]),
        //                               Add = Convert.ToBoolean(dr["IsAdd"]),
        //                               Delete = Convert.ToBoolean(dr["IsDelete"]),
        //                               Update = Convert.ToBoolean(dr["IsUpdate"]),
        //                               Visible = Convert.ToBoolean(dr["IsVisible"])
        //                           }).ToList();


        //                // Assign modules to the user
        //                user.Modules = modules;
        //            }
        //            Assertions.IsNotNull(user, Resources.Resource.DATABASE_ERROR_1004);

        //            return user;
        //        }
        //        else
        //            throw new UnauthorizedAccessException(Resource.INVALID_TOKEN);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //    }
        //    finally
        //    {
        //        if (objSQL != null) objSQL.Dispose();
        //        if (objCmd != null) objCmd.Dispose();
        //    }
        //}


        public async Task<UserProfileDto> GetUserById(long userId)
        {
            using (SQLManager objSQL = new SQLManager(_configuration))
            {
                using (SqlCommand objCmd = new SqlCommand("sp_GetUserById"))
                {
                    objCmd.Parameters.AddWithValue("@Id", userId);

                    try
                    {
                        DataSet ds = await objSQL.FetchDB(objCmd);

                        if (ds?.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                        {
                            var user = (from DataRow dr in ds.Tables[0].Rows
                                        select new UserProfileDto
                                        {
                                            Id = Convert.ToInt64(dr["Id"]),
                                            FirstName = dr["FirstName"].ToString(),
                                            LastName = dr["LastName"].ToString(),
                                            Phone = dr["Phone"].ToString(),
                                            Email = dr["Email"].ToString(),
                                            Country = dr["Country"].ToString(),
                                            CountryId = dr["CountryId"] != DBNull.Value ? Convert.ToInt32(dr["CountryId"]) : null,
                                            OrganizationId = dr["OrganizationId"] == DBNull.Value ? (long?)null : Convert.ToInt64(dr["OrganizationId"]),
                                            Status = StatusExtensions.ToStatusString((Status)Convert.ToInt32(dr["Status"])),
                                            CreatedDate = Convert.ToDateTime(dr["CreatedDate"]),
                                            UpdatedDate = dr["UpdatedDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["UpdatedDate"]),
                                            RoleId = Convert.ToInt32(dr["RoleId"]),
                                            Role = dr["Role"].ToString(),
                                            FcmToken = dr["FcmToken"].ToString(),
                                            CustomerOrganizationTypeId = dr["OrganizationTypeId"] != DBNull.Value ? Convert.ToInt32(dr["OrganizationTypeId"]) : (int?)null,
                                            CustomerOrganizationType = Convert.ToString(dr["OrganizationType"]),
                                            CustomerOrganizationName = Convert.ToString(dr["OrganizationName"])
                                            //CustomerOrganization = dr["OrganizationTypeId"] != DBNull.Value ? new CustomerOrganization
                                            //{
                                            //    Id = Convert.ToInt32(dr["OrganizationTypeId"]),
                                            //    Type = dr["OrganizationType"].ToString(),
                                            //    Name = dr["OrganizationName"].ToString(),
                                            //} : null,
                                        }).FirstOrDefault();

                            if (ds.Tables.Count > 1 && ds.Tables[1].Rows.Count > 0)
                            {
                                user.Modules = (from DataRow dr in ds.Tables[1].Rows
                                                select new Modules
                                                {
                                                    Id = Convert.ToInt32(dr["Id"]),
                                                    Name = Convert.ToString(dr["Name"]),
                                                    Add = Convert.ToBoolean(dr["IsAdd"]),
                                                    Delete = Convert.ToBoolean(dr["IsDelete"]),
                                                    Update = Convert.ToBoolean(dr["IsUpdate"]),
                                                    View = Convert.ToBoolean(dr["IsVisible"])
                                                }).ToList();
                            }

                            Assertions.IsNotNull(user, Resources.Resource.DATABASE_ERROR_1004);
                            return user;
                        }
                        else
                        {
                            throw new UnauthorizedAccessException(Resource.INVALID_TOKEN);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }

                }
            }
        }


        public async Task<UserDetailsDto> GetUserDetailsById(long userId)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetUserById");

            try
            {
                objCmd.Parameters.AddWithValue("@Id", userId);
                DataTable dt = await objSQL.FetchDT(objCmd);
                if (dt.Rows.Count > 0)
                {
                    var user = (from DataRow dr in dt.Rows
                                select new UserDetailsDto
                                {
                                    Id = Convert.ToInt64(dr["Id"]),
                                    FirstName = dr["FirstName"].ToString(),
                                    LastName = dr["LastName"].ToString(),
                                    Phone = dr["Phone"].ToString(),
                                    Email = dr["Email"].ToString(),
                                    Country = dr["Country"].ToString(),
                                    CountryId = dr["CountryId"] != DBNull.Value ? Convert.ToInt32(dr["CountryId"]) : null,
                                    OrganizationId = dr["OrganizationId"] == DBNull.Value ? (long?)null : Convert.ToInt64(dr["OrganizationId"]),
                                    CustomerOrganizationType = dr["OrganizationTypeId"].ToString(),
                                    CustomerOrganizationName = dr["OrganizationName"].ToString(),
                                    Status = StatusExtensions.ToStatusString((Status)Convert.ToInt32(dr["Status"])),
                                    RoleId = Convert.ToInt32(dr["RoleId"]),
                                    Role = dr["Role"].ToString(),
                                }).FirstOrDefault();
                    return user;
                }
                else
                    throw new UnauthorizedAccessException(Resource.INVALID_TOKEN);
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

        //public async Task<AdminProfileDto> GetAdminProfileById(long userId)
        //{
        //    SQLManager objSQL = new SQLManager(_configuration);
        //    SqlCommand objCmd = new SqlCommand("sp_GetAdminProfileById");

        //    try
        //    {
        //        objCmd.Parameters.AddWithValue("@Id", userId);
        //        DataSet ds = await objSQL.FetchDB(objCmd);
        //        var modules = new List<Modules>();

        //        // Initialize an empty AdminProfileDto object
        //        AdminProfileDto user = null;

        //        // Check if the first result set (Admin Profile) has rows
        //        if (ds.Tables[0].Rows.Count > 0)
        //        {
        //            // Map the Admin Profile details
        //            user = (from DataRow dr in ds.Tables[0].Rows
        //                    select new AdminProfileDto
        //                    {
        //                        Id = Convert.ToInt64(dr["Id"]),
        //                        FirstName = dr["FirstName"].ToString(),
        //                        LastName = dr["LastName"].ToString(),
        //                        Phone = dr["Phone"].ToString(),
        //                        Email = dr["Email"].ToString(),
        //                        Country = dr["Country"].ToString(),
        //                        CountryId = dr["CountryId"] != DBNull.Value ? Convert.ToInt32(dr["CountryId"]) : null,
        //                        Status = StatusExtensions.ToStatusString((Status)Convert.ToInt32(dr["Status"])),
        //                        RoleId = Convert.ToInt32(dr["RoleId"]),
        //                        Role = dr["Role"].ToString(),
        //                        OrganizationId = Convert.ToInt64(dr["OrganizationId"]),
        //                        FcmToken = dr["FcmToken"].ToString()
        //                    }).FirstOrDefault();

        //            // Check if the second result set (Role-wise Modules) has rows
        //            if (ds.Tables[1].Rows.Count > 0)
        //            {
        //                modules = (from DataRow dr in ds.Tables[1].Rows
        //                           select new Modules
        //                           {
        //                               Id = Convert.ToInt32(dr["Id"]),
        //                               Name = Convert.ToString(dr["Name"]),
        //                               Add = Convert.ToBoolean(dr["IsAdd"]),
        //                               Delete = Convert.ToBoolean(dr["IsDelete"]),
        //                               Update = Convert.ToBoolean(dr["IsUpdate"]),
        //                               Visible = Convert.ToBoolean(dr["IsVisible"])
        //                           }).ToList();


        //                // Assign modules to the user
        //                user.Modules = modules;
        //            }
        //            Assertions.IsNotNull(user, Resources.Resource.DATABASE_ERROR_1004);

        //            return user;
        //        }
        //        else
        //        {
        //            throw new UnauthorizedAccessException(Resource.INVALID_TOKEN);
        //        }

        //        return user;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //    }
        //    finally
        //    {
        //        if (objSQL != null) objSQL.Dispose();
        //        if (objCmd != null) objCmd.Dispose();
        //    }
        //}


        public async Task<AdminProfileDto> GetAdminProfileById(long userId)
        {
            using (SQLManager objSQL = new SQLManager(_configuration))
            {
                using (SqlCommand objCmd = new SqlCommand("sp_GetAdminProfileById"))
                {
                    objCmd.Parameters.AddWithValue("@Id", userId);

                    try
                    {
                        DataSet ds = await objSQL.FetchDB(objCmd);

                        if (ds?.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                        {
                            // Map the Admin Profile details
                            var user = (from DataRow dr in ds.Tables[0].Rows
                                        select new AdminProfileDto
                                        {
                                            Id = Convert.ToInt64(dr["Id"]),
                                            FirstName = dr["FirstName"].ToString(),
                                            LastName = dr["LastName"].ToString(),
                                            Phone = dr["Phone"].ToString(),
                                            Email = dr["Email"].ToString(),
                                            Country = dr["Country"].ToString(),
                                            CountryId = dr["CountryId"] != DBNull.Value ? Convert.ToInt32(dr["CountryId"]) : null,
                                            Status = StatusExtensions.ToStatusString((Status)Convert.ToInt32(dr["Status"])),
                                            RoleId = Convert.ToInt32(dr["RoleId"]),
                                            Role = dr["Role"].ToString(),
                                            OrganizationId = Convert.ToInt64(dr["OrganizationId"]),
                                            FcmToken = dr["FcmToken"].ToString()
                                        }).FirstOrDefault();

                            // Check if the second result set (Role-wise Modules) has rows
                            if (ds.Tables.Count > 1 && ds.Tables[1].Rows.Count > 0)
                            {
                                user.Modules = (from DataRow dr in ds.Tables[1].Rows
                                                select new Modules
                                                {
                                                    Id = Convert.ToInt32(dr["Id"]),
                                                    Name = Convert.ToString(dr["Name"]),
                                                    Add = Convert.ToBoolean(dr["IsAdd"]),
                                                    Delete = Convert.ToBoolean(dr["IsDelete"]),
                                                    Update = Convert.ToBoolean(dr["IsUpdate"]),
                                                    View = Convert.ToBoolean(dr["IsVisible"])
                                                }).ToList();
                            }

                            Assertions.IsNotNull(user, Resources.Resource.DATABASE_ERROR_1004);
                            return user;
                        }
                        else
                        {
                            throw new UnauthorizedAccessException(Resource.INVALID_TOKEN);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log or handle the exception as needed
                        throw;
                    }
                }
            }
        }

        public async Task<UserListDto> GetAllUsers(long organizationId, int pageNo = 1, int pageSize = 10, string sortColumn = "", string sortOrder = "", string searchText = null)
        {
            List<UserResponseDto> users = new List<UserResponseDto>();
            var totalCount = 0;
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetAllUsers");

            try
            {
                objCmd.Parameters.AddWithValue("@OrganizationId", organizationId);
                objCmd.Parameters.AddWithValue("@PageNumber", pageNo);
                objCmd.Parameters.AddWithValue("@PageSize", pageSize);
                objCmd.Parameters.AddWithValue("@SearchText", searchText);
                objCmd.Parameters.AddWithValue("@SortOrder", sortOrder);
                objCmd.Parameters.AddWithValue("@SortColumn", sortColumn);

                DataTable dt = await objSQL.FetchDT(objCmd);

                if (dt.Rows.Count > 0)
                {
                    users = (from DataRow dr in dt.Rows
                                select new UserResponseDto
                                {
                                    Id = Convert.ToInt64(dr["Id"]),
                                    FirstName = dr["FirstName"].ToString(),
                                    LastName = dr["LastName"].ToString(),
                                    Phone = dr["Phone"].ToString(),
                                    Email = dr["Email"].ToString(),
                                    Country = dr["Country"].ToString(),
                                    CountryId = dr["CountryId"] != DBNull.Value ? Convert.ToInt32(dr["CountryId"]) : 0,
                                    OrganizationType = dr["OrganizationType"].ToString(),
                                    OrganizationName = dr["OrganizationName"].ToString(),
                                    Status = StatusExtensions.ToStatusString((Status)Convert.ToInt32(dr["Status"])), 
                                    CreatedDate = Convert.ToDateTime(dr["CreatedDate"]),
                                    UpdatedDate = dr["UpdatedDate"] != DBNull.Value ? Convert.ToDateTime(dr["UpdatedDate"]) : (DateTime?)null,
                                }).ToList();

                    totalCount = Convert.ToInt32(dt.Rows[0]["TotalCount"]);
                }
                return new UserListDto { List = users, TotalCount = totalCount };
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

        public async Task<UserResponseDto> GetCustomerDetailsById(long customerId, long organizationId)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_CustomerDetails");

            try
            {
                UserResponseDto users = null;
                objCmd.Parameters.AddWithValue("@CustomerId", customerId);
                objCmd.Parameters.AddWithValue("@OrganizationId", organizationId);

                DataTable dt = await objSQL.FetchDT(objCmd);
                if (dt.Rows.Count > 0)
                {
                    users = (from DataRow dr in dt.Rows
                             select new UserResponseDto
                             {
                                 Id = Convert.ToInt64(dr["Id"]),
                                 FirstName = dr["FirstName"].ToString(),
                                 LastName = dr["LastName"].ToString(),
                                 Phone = dr["Phone"].ToString(),
                                 Email = dr["Email"].ToString(),
                                 Country = dr["Country"].ToString(),
                                 CountryId = dr["CountryId"] != DBNull.Value ? Convert.ToInt32(dr["CountryId"]) : 0,
                                 OrganizationType = dr["OrganizationType"].ToString(),
                                 OrganizationName = dr["OrganizationName"].ToString(),
                                 Status = StatusExtensions.ToStatusString((Status)Convert.ToInt32(dr["Status"])),
                                 CreatedDate = Convert.ToDateTime(dr["CreatedDate"]),
                                 UpdatedDate = dr["UpdatedDate"] != DBNull.Value ? Convert.ToDateTime(dr["UpdatedDate"]) : (DateTime?)null,
                             }).FirstOrDefault();

                    return users;
                }
                Assertions.IsNotNull(users, Resources.Resource.CUSTOMER_NOT_FOUND);

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

        public async Task<string> UpdateUserFcmTokenAsync(FcmTokenInput input)
        {
         
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_UpdateUserFcmToken");

            try
            {
                objCmd.Parameters.AddWithValue("@UserId", input.UserId);
                objCmd.Parameters.AddWithValue("@FcmToken", input.FcmToken);

                DataTable dt = await objSQL.FetchDT(objCmd);

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

    }
}
