using FirebaseAdmin.Messaging;

namespace EventManagement.Utilities.FireBase
{
    public interface IFirebaseServices
    {
        Task<string> SendFCM(MulticastMessage message);
        Task<string> SendFirebaseNotification(string[] receiverTokenIds, string title, string message, bool isForWeb, int badgeCount = 0);
    }
}
