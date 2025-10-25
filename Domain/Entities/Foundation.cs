namespace Trustesse.Ivoluntia.Domain.Entities
{
    public class Foundation : BaseEntity
    {
        public string Name { get; set; }
        public string CategoryId { get; set; }
        public FoundationCategory Category { get; set; }
        public string Mission { get; set; }
        public string? Logo { get; set; }
        public string? Website { get; set; }
        public Location Location { get; set; }
        public string LocationId { get; set; }
        public string Email { get; set; }
        public DateTime YearEstablished { get; set; }
        public bool IsActive { get; set; }
        public bool HasAgreedToDisclaimer { get; set; }
        public ICollection<User> Admins { get; set; } = new List<User>();
        public ICollection<Cause> Causes { get; set; } = new List<Cause>();
        public ICollection<Program> Programs { get; set; } = new List<Program>();
    }
}
