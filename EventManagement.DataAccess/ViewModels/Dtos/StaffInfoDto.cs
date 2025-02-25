
namespace EventManagement.DataAccess.ViewModels.Dtos
{
    public class StaffInfoDto
    {
        public long Id { get; set; }
        public long StaffId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Role { get; set; }
    }
}
