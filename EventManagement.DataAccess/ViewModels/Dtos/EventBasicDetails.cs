
using EventManagement.DataAccess.ViewModels.ApiObjects;

namespace EventManagement.DataAccess.ViewModels.Dtos
{
    public class EventBasicDetails
    {
        public long Id { get; set; }
        public string Name { get; set; }
        //public List<PaymentMethodSupported> PaymentMethodSupported { get; set; } = new List<PaymentMethodSupported>();
        public string BannerImage { get; set; }
        //public string Description { get; set; }
        public string Address { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }

    }
}
