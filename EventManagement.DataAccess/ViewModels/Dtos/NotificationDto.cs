namespace EventManagement.DataAccess.ViewModels.Dtos
{
    public class NotificationDto
    {
        public long Id { get; set; }
        public int NotificationTypeId { get; set; }
        public int ActionType { get; set; }
        public long OrganizationId { get; set; }
        public long UserId { get; set; }
        public string NotificationDateTime { get; set; }
        public string MessageTitle { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; }
        public string CreatedDate { get; set; }
    }
}
