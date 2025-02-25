using EventManagement.DataAccess.Models;
using EventManagement.DataAccess.ViewModels.ApiObjects;

namespace EventManagement.DataAccess.ViewModels.Dtos
{
    public class AdminProfileDto
    {
        public long Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public int? CountryId { get; set; }
        public string Country { get; set; }
        public string Status { get; set; }
        public string Role { get; set; }
        public int RoleId { get; set; }
        public long OrganizationId { get; set; }
        public string FcmToken { get; set; }
        public List<Modules> Modules { get; set; }
    }
}
