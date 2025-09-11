namespace Trustesse.Ivoluntia.Domain.Entities;

public class UserInterestLink : BaseEntity
{
    public string UserId { get; set; }
    public string InterestId { get; set; }
    public virtual User User { get; set; }
    public virtual Interest Interest { get; set; }
   
}