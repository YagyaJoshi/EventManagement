namespace EventManagement.DataAccess.ViewModels.Dtos
{
    public class StaffDetailsDto
    {
        public long Id { get; set; }

        public long OrganizationId {  get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Phone { get; set; }

        public string Email { get; set; }

        public string Status { get; set; }

        //public string Password { get; set; } = string.Empty;
        public int RoleId { get; set; }

        public string Role { get; set; }
    }
}
