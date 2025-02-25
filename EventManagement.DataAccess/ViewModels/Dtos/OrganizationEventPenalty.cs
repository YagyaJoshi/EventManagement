

namespace EventManagement.DataAccess.ViewModels.Dtos
{
    public class OrganizationEventPenalty
    {
        public long Id { get; set; }
        public long EventId { get; set; }
        public int PenaltyType { get; set; }
        public DateTime Deadline { get; set; }
        public decimal Fees { get; set; }
        public int CurrencyId { get; set; }
        public bool IsPercentage { get; set; }
    }
    public class Penalties
    {
        public int PenaltyType { get; set; }
        public string Deadline { get; set; }
        public decimal Fees { get; set; }
        public int CurrencyId { get; set; }
        public bool IsPercentage { get; set; }
    }

    public class PenaltiesInfo
    {
        public long Id { get; set; }
        public int PenaltyType { get; set; }

        public string OrganizationName { get; set; }
        public string Deadline { get; set; }
        public decimal Fees { get; set; }
        public int CurrencyId { get; set; }
        public bool IsPercentage { get; set; }

        public string CurrencyCode { get; set; }

        public string MerchantId { get; set; }

        public string ApiPassword { get; set; }

        public decimal DisplayCurrencyRate { get; set; }

        public string PaymentUrl { get; set; }

        public string EventName { get; set; }

        public int ApiVersion { get; set; }
    }

    public class PenaltiesDetail
    {
        public int PenaltyType { get; set; }
        public string Deadline { get; set; }
        public decimal Fees { get; set; }
        public int CurrencyId { get; set; }
        public bool IsPercentage { get; set; }
        public long EventId { get; set; }
    }
}
