namespace EventManagement.Utilities.Payment.Mastercard
{
    public class Interaction
    {
        public string operation { get; set; }
        public Merchant merchant { get; set; }
        public string returnUrl { get; set; }

        public DisplayControl displayControl { get; set; }
    }

    public class DisplayControl
    {
        public string billingAddress { get; set; }
    }

    public class Merchant
    {
        public string name { get; set; }
        public string url { get; set; }
    }

    public class Order
    {
        public string currency { get; set; }
        public string amount { get; set; }
        public string id { get; set; }
        public string description { get; set; }
    }

    public class CreateSessionRequest
    {
        public string apiOperation { get; set; }
        public string checkoutMode { get; set; }
        public Interaction interaction { get; set; }
        public Order order { get; set; }
    }

}
