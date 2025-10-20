namespace Trustesse.Ivoluntia.Domain.Entities
{
    public class ProgramSkill : BaseEntity
    {
        public string ProgramId { get; set; }
        public string SkillId { get; set; }

        public Program Program { get; set; }
        public Skill Skill { get; set; }
    }
}
