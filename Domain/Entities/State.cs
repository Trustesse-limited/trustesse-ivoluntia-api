using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Trustesse.Ivoluntia.Domain.Entities;

public class State : BaseEntity
{
    public string StateName { get; set; }
    public string CountryId { get; set; }
    [ForeignKey("CountryId")]
    public virtual Country Country { get; set; }
}