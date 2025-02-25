using EventManagement.DataAccess.Enums;
using EventManagement.DataAccess.Extensions;
using EventManagement.DataAccess.ViewModels.ApiObjects;
using EventManagement.DataAccess.ViewModels.Dtos;
using System.Data;

namespace EventManagement.BusinessLogic.Services.v1.Mappings
{
    public static class StaffMappings
    {
        public static StaffDetailsDto MapToDto(DataRow dr)
        {
            return new StaffDetailsDto
            {
                Id = Convert.ToInt64(Convert.ToString(dr["StaffId"])),
                FirstName = Convert.ToString(dr["FirstName"]),
                LastName = Convert.ToString(dr["LastName"]),
                Phone = Convert.ToString(dr["Phone"]),
                Email = Convert.ToString(dr["Email"]),
                Status = StatusExtensions.ToStatusString((Status)Convert.ToInt32(Convert.ToString(dr["Status"]))),
                OrganizationId = Convert.ToInt64(Convert.ToString(dr["OrganizationId"])),
                RoleId = Convert.ToInt32(Convert.ToString(dr["RoleId"])),
                Role = Convert.ToString(dr["Role"])
            };
        }
    }
}
