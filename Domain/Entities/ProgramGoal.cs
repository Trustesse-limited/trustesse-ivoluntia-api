namespace Trustesse.Ivoluntia.Domain.Entities
{
    public class ProgramGoal : BaseEntity
    {
        public string ProgramId { get; set; }
        public Program Program { get; set; }
        public string Goal { get; set; }
        public bool IsAchieved { get; set; } = false;
    }
}
