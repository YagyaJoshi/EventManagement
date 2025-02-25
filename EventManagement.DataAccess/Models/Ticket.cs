using EventManagement.DataAccess.ViewModels.Dtos;

namespace EventManagement.DataAccess.Models
{
    public class Ticket
    {
        public long Id { get; set; }
        public long OrganizationId { get; set; }
        public long UserId { get; set; }
        public string? Subject { get; set; }
        public string Message { get; set; }
        public string ReplyMessage { get; set; }
        public long? AssignedToId { get; set; }
        public long? AssignedById { get; set; }
        public long? ParentTicketId { get; set; }
        public string Status { get; set; }       
        public DateTimeOffset? CreatedAt { get; set; }
        public DateTimeOffset? ClosedAt { get; set; }
        public UserInfo User { get; set; }
    }

    public class UserInfo
    {
        public long Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int RoleId { get; set; }
        public string Role { get; set; }
    }
}
