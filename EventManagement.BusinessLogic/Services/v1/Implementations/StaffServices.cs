using EventManagement.BusinessLogic.Services.v1.Abstractions;
using EventManagement.DataAccess.ViewModels.ApiObjects;
using EventManagement.DataAccess;
using EventManagement.Utilities.Helpers;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Data;
using EventManagement.DataAccess.ViewModels.Dtos;
using EventManagement.Utilities.Email;
using EventManagement.BusinessLogic.Helpers;
using EventManagement.BusinessLogic.Exceptions;
using EventManagement.DataAccess.Extensions;
using EventManagement.BusinessLogic.Services.v1.Mappings;
using Microsoft.Extensions.Hosting;
using EventManagement.DataAccess.Models;
using System.Drawing.Printing;

namespace EventManagement.BusinessLogic.Services.v1.Implementations
{
    public class StaffServices : IStaffServices
    {
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly IHostEnvironment _env;

        public StaffServices(IConfiguration configuration, IEmailService emailService, IHostEnvironment env)
        {
            _configuration = configuration;
            _emailService = emailService;
            _env = env;
        }

        public async Task<long> AddorUpdateStaff(long? id, long organizationId, AddStaffInput input)
        {
           
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_AddOrUpdateStaff");
            try
            {
                // hash password
                var PasswordHash = PasswordHasher.HashPassword(input.Password);

                if (id.HasValue)
                    objCmd.Parameters.AddWithValue("@Id", id);

                var status = StatusExtensions.ToStatusEnum(input.Status.ToLower()) ?? throw new ServiceException($"Invalid status: {input.Status}");

                objCmd.Parameters.AddWithValue("@FirstName", input.FirstName);
                objCmd.Parameters.AddWithValue("@LastName", input.LastName);
                objCmd.Parameters.AddWithValue("@Email", input.Email.ToLower());
                objCmd.Parameters.AddWithValue("@Password", PasswordHash);
                objCmd.Parameters.AddWithValue("@Phone", input.Phone);
                objCmd.Parameters.AddWithValue("@Status", status);
                objCmd.Parameters.AddWithValue("@OrganizationId", organizationId);
                objCmd.Parameters.AddWithValue("@RoleId", input.RoleId);

                DataTable dtUser = await objSQL.FetchDT(objCmd);

                var error = Convert.ToInt64(dtUser.Rows[0]["ErrorCode"]);
                var errorMessage = CommonUtilities.GetErrorMessage(error);
                if (!string.IsNullOrEmpty(errorMessage))
                    throw new ServiceException(errorMessage);

                string logo = dtUser.Rows[0]["Logo"] != DBNull.Value ? dtUser.Rows[0]["Logo"].ToString() : null;
                string Name = Convert.ToString(dtUser.Rows[0]["Name"]);

                if (!id.HasValue)
                {
                    string folderPath = "EmailTemplates";
                    string fileName = "staff-register.html";
                    string filePath = Path.Combine(folderPath, fileName);

                    var registrationEmailText = CommonUtilities.GetEmailTemplateText(_env.ContentRootPath + Path.DirectorySeparatorChar.ToString() + "EmailTemplates" + Path.DirectorySeparatorChar.ToString() + "staff-register.html");

                    registrationEmailText = string.Format(registrationEmailText, input.Email, input.Password, logo, Name);

                    if (input.Email != null)
                        _emailService.SendEmail(input.Email, "Welcome", registrationEmailText);
                }

                return Convert.ToInt64(dtUser.Rows[0]["StaffId"]);
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

        public async Task<StaffListDto> GetAllStaffs(long organizationId,string sortColumn, string sortOrder, string searchText, int? pageNo, int? pageSize)
        {
            var staffs = new List<StaffDetailsDto>();
            int totalCount = 0;
            int totalRecords = 0;

            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetAllStaff");

            try
            {
                objCmd.CommandType = CommandType.StoredProcedure;

                objCmd.Parameters.AddWithValue("@OrganizationId", organizationId);
                objCmd.Parameters.AddWithValue("@PageNumber", pageNo);
                objCmd.Parameters.AddWithValue("@PageSize", pageSize);
                objCmd.Parameters.AddWithValue("@SearchText", searchText);
                objCmd.Parameters.AddWithValue("@SortOrder", sortOrder);
                objCmd.Parameters.AddWithValue("@SortColumn", sortColumn);

                DataTable dt = await objSQL.FetchDT(objCmd);

                if (dt.Rows.Count > 0)
                {
                    staffs = (from DataRow dr in dt.Rows
                                  select StaffMappings.MapToDto(dr)).ToList();   
                    
                    totalCount = Convert.ToInt32(dt.Rows[0]["TotalCount"]);
                    totalRecords = Convert.ToInt32(dt.Rows[0]["TotalRecords"]);
                }
                return new StaffListDto{ List = staffs, TotalCount = totalCount, TotalRecords = totalRecords }; 
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

        public async Task< StaffDetailsDto> GetStaffById(long id)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetStaffById");

            try
            {
                objCmd.Parameters.AddWithValue("@StaffId", id);
                DataTable dt = await objSQL.FetchDT(objCmd);

                var staff = (from DataRow dr in dt.Rows
                              select StaffMappings.MapToDto(dr)).FirstOrDefault();

                Assertions.IsNotNull(staff, Resources.Resource.DATABASE_ERROR_1038);

                return staff;

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

        public async Task<long> DeleteStaff(long id)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_DeleteStaff");

            try
            {
                objCmd.Parameters.AddWithValue("@Id", id);
                DataTable dt = await objSQL.FetchDT(objCmd);

                var error = Convert.ToInt64(dt.Rows[0]["ErrorCode"]);
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

        public async Task<string> UpdateVisaDocument(long bookingId, long guestId, string url)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_UpdateVisaDocument");

            try
            {       
                objCmd.CommandType = CommandType.StoredProcedure;
                objCmd.Parameters.AddWithValue("@GuestId", guestId);
                objCmd.Parameters.AddWithValue("@OrderId", bookingId);
                objCmd.Parameters.AddWithValue("@Url", url);
                await objSQL.UpdateDB(objCmd);
                return url;
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

        public async Task<List<DataAccess.Models.Roles>> GetStaffRoles()
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetStaffRoles");

            try
            {
                DataTable dt = await objSQL.FetchDT(objCmd);

                var staffs = (from DataRow dr in dt.Rows
                             select new DataAccess.Models.Roles
                             {
                                 Id = Convert.ToInt32(dr["Id"]),
                                 Name = Convert.ToString(dr["Name"])
                             }).ToList();

                return staffs;

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

        public async Task<List<StaffInfoDto>> GetAssignStaffs(long organizationId)
        {
            var staffs = new List<StaffInfoDto>();
            int totalCount = 0;

            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetListofAssignStaff");

            try
            {
                objCmd.CommandType = CommandType.StoredProcedure;
                objCmd.Parameters.AddWithValue("@OrganizationId", organizationId);

                DataTable dt = await objSQL.FetchDT(objCmd);

                if (dt.Rows.Count > 0)
                {
                    staffs = (from DataRow dr in dt.Rows
                              select new StaffInfoDto
                              {
                                  Id = Convert.ToInt64(dr["UserId"]),
                                  StaffId = Convert.ToInt64(dr["StaffId"]),
                                  FirstName = dr["FirstName"].ToString(),
                                  LastName = dr["LastName"].ToString(),
                                  Role = Convert.ToString(dr["Role"])
                              }).ToList();
                }
                return staffs;
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
