using EventManagement.BusinessLogic.Helpers;
using EventManagement.BusinessLogic.Services.v1.Abstractions;
using EventManagement.DataAccess;
using EventManagement.DataAccess.ViewModels.ApiObjects;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Data;
using EventManagement.DataAccess.Extensions;
using EventManagement.DataAccess.ViewModels.Dtos;
using EventManagement.DataAccess.Enums;

namespace EventManagement.BusinessLogic.Services.v1.Implementations
{
    public class AdminServices : IAdminServices
    {
        private readonly IConfiguration _configuration;

        public AdminServices(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<long> AddAccreditationTemplate(long organizationId,AccreditationInput input)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_AddAccreditationTemplate");
            try
            {
                objCmd.Parameters.AddWithValue("@OrganizationId", organizationId);
                objCmd.Parameters.AddWithValue("@TemplateImage", input.TemplateImage);
                objCmd.Parameters.AddWithValue("@Instruction", input.Instruction);
                objCmd.Parameters.AddWithValue("@Status", Accreditation.pending);

                DataTable dtAccreditation = await objSQL.FetchDT(objCmd);

                return Convert.ToInt64(dtAccreditation.Rows[0]["Id"]);
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

        public async Task<AccreditationListDto> GetAccreditationTemplate(long? organizationId, int? pageNo, int? pageSize, string? sortOrder)
        {
            SQLManager objSQL = new SQLManager(_configuration);
            SqlCommand objCmd = new SqlCommand("sp_GetAccreditationTemplates");

            try
            {
                if(organizationId > 0)
                    objCmd.Parameters.AddWithValue("@OrganizationId", organizationId);
                objCmd.Parameters.AddWithValue("@PageNo", pageNo);
                objCmd.Parameters.AddWithValue("@PageSize", pageSize);
                objCmd.Parameters.AddWithValue("@SortOrder", sortOrder);

                DataTable dt = await objSQL.FetchDT(objCmd);
                List<AccreditationDto> accreditation = new List<AccreditationDto>();
                int totalCount = 0;
                if (dt.Rows.Count > 0)
                {
                    accreditation = (from DataRow dr in dt.Rows
                              select new AccreditationDto
                              {
                                  Id = Convert.ToInt64(dr["Id"]),
                                  OrganizationId = Convert.ToInt64(dr["OrganizationId"]),
                                  TemplateImage = Convert.ToString(dr["TemplateImage"]),
                                  Instruction = Convert.ToString(dr["Instruction"]),
                                  Status = StatusExtensions.ToStatusString((Status)Convert.ToInt32(dr["Status"]))
                              }).ToList();

                    totalCount = Convert.ToInt32(dt.Rows[0]["TotalCount"]);

                }

                return new AccreditationListDto { List = accreditation, TotalCount = totalCount };
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
