using EventManagement.DataAccess.Models;

namespace EventManagement.DataAccess.ViewModels.Dtos
{
    public class OrganizationListDto
    {
        public List<Organization> List { get; set; } = new List<Organization>();
        public int TotalCount { get; set; }
    }
}
