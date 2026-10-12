namespace Trustesse.Ivoluntia.Domain.Entities;

public class UserSkillLink : BaseEntity
{
    public string UserId { get; set; }
    public User User { get; set; }
    public string SkillId { get; set; }
    public Skill Skill { get; set; }
}