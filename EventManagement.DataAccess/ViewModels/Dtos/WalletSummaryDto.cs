
namespace EventManagement.DataAccess.ViewModels.Dtos
{
    public class WalletSummaryDto
    {
        public long Id { get; set; }
        public long? OrderId { get; set; }
        public decimal? Amount { get; set; }
        public DateTime? Date { get; set; }
        public string Status { get; set; }

        public string Notes { get; set; }
    }
}
