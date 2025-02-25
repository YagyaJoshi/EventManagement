
namespace EventManagement.DataAccess.ViewModels.ApiObjects
{
    public class ReplaceGuestInput
    {
        public long OldGuestId { get; set; }
        public long? NewGuestId { get; set;}
        public long OrderId { get; set; }
        public string SuccessUrl { get; set; }
        public string CancelUrl { get; set; }
        public string? PassportFirstName { get; set; }
        public string? PassportLastName { get; set; }
        public string? PassportNumber { get; set; }
    }
}
