
namespace EventManagement.DataAccess.ViewModels.Dtos
{
    public class OrderPenaltyDto
    {
        public int Id { get; set; }
        public int PenaltyType { get; set; }
        public string PenaltyName { get; set; }
        public Decimal? Amount { get; set; }
    }
}
