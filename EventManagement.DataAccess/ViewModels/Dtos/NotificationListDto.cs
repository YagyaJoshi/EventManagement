namespace EventManagement.DataAccess.ViewModels.Dtos
{
    public class NotificationListDto
    {
        public List<NotificationDto> List { get; set; } = new List<NotificationDto>();
        public int TotalCount { get; set; }
    }
}
