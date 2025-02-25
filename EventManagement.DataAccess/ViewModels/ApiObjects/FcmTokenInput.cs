

namespace EventManagement.DataAccess.ViewModels.ApiObjects
{
    public class FcmTokenInput
    {
        public long UserId { get; set; }
        public string FcmToken { get; set; } = string.Empty;
    }
}
