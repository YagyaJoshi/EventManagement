using EventManagement.DataAccess.Enums;

namespace EventManagement.DataAccess.Extensions
{
    public  class StatusExtensions
    {
        public static string ToStatusString(Status status)
        {
            return status.ToString().ToLower();
        }

        public static Status? ToStatusEnum(string statusString)
        {
            if (Enum.TryParse(typeof(Status), statusString, true, out var status))
            {
                return (Status)status;
            }
            return null;
        }

        public static VisaStatus? ToVisaStatusEnum(string statusString)
        {
            if (Enum.TryParse(typeof(VisaStatus), statusString, true, out var status))
            {
                return (VisaStatus)status;
            }
            return null;
        }

        public static ProfileStatus? ToGuestStatusEnum(string statusString)
        {
            if (Enum.TryParse(typeof(ProfileStatus), statusString, true, out var status))
            {
                return (ProfileStatus)status;
            }
            return null;
        }
    }
}
