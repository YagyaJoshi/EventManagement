namespace EventManagement.DataAccess.ViewModels.Dtos
{
    public class WalletDetailDto
    {
        public long Id { get; set; }

        public decimal Amount { get; set; } = 0;

        public long UserId { get; set; }

        public long OrganizationId { get; set; }

        public string Notes { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }
    }
}
