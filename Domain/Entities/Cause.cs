namespace Trustesse.Ivoluntia.Domain.Entities
{
    public class Cause : BaseEntity
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public ICollection<Foundation?> Foundations { get; set; } = new List<Foundation>();
    }
}
