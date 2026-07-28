namespace Trustesse.Ivoluntia.Domain.Entities
{
    public class TransactionPin : BaseEntity
    {
        public string UserId { get; set; }
        public string PinHash { get; set; }
        public DateTime CreatedDate { get; set; }

        public virtual User User { get; set; } = null!;
    }
}
