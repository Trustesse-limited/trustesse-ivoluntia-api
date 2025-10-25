namespace Trustesse.Ivoluntia.Domain.Entities
{
    public class Interest : BaseEntity
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public ICollection<User> Users { get; set; } = new List<User>();
        public ICollection<UserInterestLink?> UserInterestLinks { get; set; } = new List<UserInterestLink>();
    }
}
