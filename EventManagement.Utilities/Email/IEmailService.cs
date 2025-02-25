namespace EventManagement.Utilities.Email
{
    public interface IEmailService
    {
        Task SendEmail(string to, string subject, string html, List<string> cc = null);
    }
}
