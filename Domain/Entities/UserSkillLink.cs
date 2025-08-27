namespace Trustesse.Ivoluntia.Domain.Entities;

public class UserSkillLink : BaseEntity
{
    public string UserId { get; set; }
    public string SkillId { get; set; }
    public virtual User User { get; set; }
    public virtual Skill Skill { get; set; }
   
}