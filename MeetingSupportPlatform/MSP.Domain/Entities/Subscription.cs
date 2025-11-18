using MSP.Domain.Base;

namespace MSP.Domain.Entities
{
    public class Subscription : BaseEntity<Guid>
    {
        public string PaymentMethod { get; set; }
        public string TransactionID { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
        public Guid PackageId { get; set; }
        public virtual Package Package { get; set; }
        public Guid UserId { get; set; }
        public virtual User User { get; set; }
    }
}
