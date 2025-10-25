namespace Trustesse.Ivoluntia.Domain.Entities
{
    public class Skill : BaseEntity
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public ICollection<User> Users { get; set; } = new List<User>();
        public ICollection<UserSkillLink?> UserSkillLinks { get; set; } = new List<UserSkillLink>();
        public ICollection<ProgramSkill> ProgramSkills { get; set; } = new List<ProgramSkill>();
    }
}
