﻿namespace EventManagement.DataAccess.ViewModels.Dtos
{
    public class UserDetailsDto
    {
        public long Id { get; set; }
        public long? OrganizationId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public int? CountryId { get; set; }
        public string Country {  get; set; }
        public string CustomerOrganizationType { get; set; }
        public string CustomerOrganizationName { get; set; }
        public string Status { get; set; }
        public string Role { get; set; }
        public int RoleId { get; set; }
    }
}
