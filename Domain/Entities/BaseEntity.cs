namespace Trustesse.Ivoluntia.Domain.Entities;

public class BaseEntity
{
    public long Id { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime DateCreated { get; set; }=DateTime.Now;
    public DateTime? DateUpdated { get; set; }
    public bool IsDeprecated { get; set; }
}