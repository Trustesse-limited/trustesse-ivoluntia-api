namespace Trustesse.Ivoluntia.Domain.Entities
{
    public class Location : BaseEntity
    {
        public string Country { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string Zipcode { get; set; }
        public string Address { get; set; }
        public string UserId { get; set; }
        public virtual User User { get; set; }
        public string FoundationId { get; set; }
        public Foundation Foundation { get; set; }
    }
}
