namespace Trustesse.Ivoluntia.Domain.Entities
{
    public class Foundation
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ContactInfo { get; set; }
        public string OrganizationSize { get; set; }
        public string Logo { get; set; }
        public string Location { get; set; }
        public string AdminEmail { get; set; }
        public DateTime YearEstablished { get; set; }
        public bool IsActive { get; set; }
        public virtual ICollection<User> Admins { get; set; } = new List<User>();
    }

}
