using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Trustesse.Ivoluntia.Domain.Entities;

public class State
{
    public long Id { get; set; }
    [Required, StringLength(100)]
    public string StateName { get; set; }
    public long CountryId { get; set; }
    [ForeignKey("CountryId")]
    public virtual Country Country { get; set; }
}