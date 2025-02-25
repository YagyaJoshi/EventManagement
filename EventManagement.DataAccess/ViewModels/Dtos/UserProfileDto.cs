using EventManagement.DataAccess.Models;

namespace EventManagement.DataAccess.ViewModels.Dtos
{
    public class UserProfileDto
    {
        public long Id { get; set; }
        public long? OrganizationId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public int? CountryId { get; set; }
        public string Country { get; set; }
        public string Status { get; set; }
        public string Role { get; set; }
        public int RoleId { get; set; }

        public string FcmToken { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }
        public List<Modules> Modules { get; set; }

        public int? CustomerOrganizationTypeId { get; set; }
        public string CustomerOrganizationType { get; set; }
        public string CustomerOrganizationName { get; set; }
        //public CustomerOrganization CustomerOrganization { get; set; }
    }

    public class CustomerOrganization
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
    }
}
