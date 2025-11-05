using MSP.Domain.Base;

namespace MSP.Domain.Entities
{
    public class Package : BaseEntity<Guid>
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; }
        public string BillingCycle { get; set; }
        public Guid CreatedById { get; set; }
        public virtual User CreatedBy { get; set; }

        public virtual ICollection<Limitation> Limitations { get; set; } = new List<Limitation>();

    }
}
