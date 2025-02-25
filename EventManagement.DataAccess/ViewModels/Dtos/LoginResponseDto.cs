using EventManagement.DataAccess.Models;

namespace EventManagement.DataAccess.ViewModels.Dtos
{
    public class LoginResponseDto
    {
        public long Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public int? CountryId { get; set; }
        public string Country { get; set; }
        public string Password { get; set; }
        public string OrganizationType { get; set; }
        public string OrganizationName { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }
        public string Token { get; set; }
        public int RoleId {  get; set; }
        public string Role { get; set; }
        public string OrganizationStatus { get; set; } = string.Empty;

    }
}
