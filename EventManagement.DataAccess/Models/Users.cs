using EventManagement.DataAccess.Enums;

namespace EventManagement.DataAccess.Models
{
    public class Users
    {
        public long Id { get; set; } 
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string CountryId { get; set; }
        public string Country { get; set; }
        public string Password { get; set; }
        public string OrganizationType { get; set; }
        public string OrganizationName { get; set; }
        public Status Status { get; set; }
        public string FcmToken { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }
    }
}
