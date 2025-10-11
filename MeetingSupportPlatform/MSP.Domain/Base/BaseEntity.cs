namespace MSP.Domain.Base
{
    /// <summary>
    /// BaseEntity
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public abstract class BaseEntity<TKey> : IBaseEntity
    {
        public TKey Id { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
