using FirebaseAdmin.Messaging;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace EventManagement.Utilities.FireBase
{
    public class FirebaseServices : IFirebaseServices
    {
        private readonly IConfiguration _config;
        private readonly IHostEnvironment _env;

        public FirebaseServices(IConfiguration config, IHostEnvironment env)
        {
            _config = config;
            _env = env;
            InitializeFirebase();
        }

        private void InitializeFirebase()
        {
            if (FirebaseApp.DefaultInstance == null)
            {
            
                var _serviceAccountPath = Path.Combine(_env.ContentRootPath, "FirebaseServiceAccountFile", "saas-9d4f1-firebase-adminsdk-hojzy-cdd0a3cddb.json");
                FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromFile(_serviceAccountPath),
                });
            }
        }

        public async Task<string> SendFCM(MulticastMessage message)
        {
            try
            {
                var response = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(message);
                return response.SuccessCount > 0 ? "Notification sent successfully." : "Failed to send notification.";
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        public async Task<string> SendFirebaseNotification(string[] receiverTokenIds, string title, string message, bool isForWeb, int badgeCount = 0)
        {
            var messagePayload = new MulticastMessage()
            {
                Tokens = receiverTokenIds,
                Notification = new Notification
                {
                    Title = title,
                    Body = message,
                },
                Data = new Dictionary<string, string>()
                {
                    { "badge", (badgeCount + 1).ToString() }
                },
                Android = new AndroidConfig
                {
                    Priority = Priority.High
                },
                Apns = new ApnsConfig
                {
                    Aps = new Aps
                    {
                        Badge = badgeCount + 1,
                        Sound = "default"
                    }
                }
            };

            return await SendFCM(messagePayload);
        }
    }
}
