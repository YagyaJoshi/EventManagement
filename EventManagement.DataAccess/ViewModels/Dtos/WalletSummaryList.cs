
namespace EventManagement.DataAccess.ViewModels.Dtos
{
    public class WalletSummaryList
    {
        public List<WalletSummaryDto> List { get; set; } = new List<WalletSummaryDto>();

        public int TotalCount { get; set; }
    }
}
