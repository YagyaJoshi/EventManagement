using EventManagement.DataAccess.Models;
using EventManagement.Utilities;
using System.Net;
using System.Resources;

namespace EventManagement.BusinessLogic.Helpers
{
    public class CommonUtilities
    {
        public static string GetEmailTemplateText(string filePath)
        {
            // Ensure the file path is absolute
            filePath = Path.GetFullPath(filePath);

            // Read the file content
            string mailText;
            using (StreamReader str = new StreamReader(filePath))
            {
                mailText = str.ReadToEnd();
            }
            return mailText;
        }


        public static string GetErrorMessage(long errorCode)
        {
            // Load the resource manager
            ResourceManager resourceManager = new ResourceManager("EventManagement.BusinessLogic.Resources.Resource", typeof(CommonUtilities).Assembly);

            // Get the error message by key
            string errorMessage = resourceManager.GetString("DATABASE_ERROR_" + errorCode.ToString());

            return errorMessage;
        }
    }
}
