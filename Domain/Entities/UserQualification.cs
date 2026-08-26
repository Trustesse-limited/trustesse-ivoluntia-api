namespace Trustesse.Ivoluntia.Domain.Entities;

public class UserQualification : BaseEntity
{
    public string UserId { get; set; }
    public string QualificationTypeId { get; set; }
    public string ProofOfQualificationURL { get; set; }

    public virtual User User { get; set; } = null!;
    public virtual Qualification Qualification { get; set; } = null!;
}
