namespace EventManagement.DataAccess.ViewModels.ApiObjects
{
    public class ReplyTicketInput
    {
        public long TicketId { get; set; }
        public string Message { get; set; } = string.Empty;
        public string ReplyMessage { get; set; } = string.Empty;
    }
}
