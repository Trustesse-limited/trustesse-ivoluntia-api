namespace Trustesse.Ivoluntia.Domain.Entities
{
    public class Skill
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public ICollection<User> Users { get; set; } = new List<User>();

    }

}
