namespace EventManagement.Utilities.Payment.Mastercard
{
    public class CreateSessionResponse
    {
        public string checkoutMode { get; set; }
        public string merchant { get; set; }
        public string result { get; set; }
        public Session session { get; set; }
        public string successIndicator { get; set; }
    }
    public class Session
    {
        public string id { get; set; }
        public string updateStatus { get; set; }
        public string version { get; set; }
    }

}