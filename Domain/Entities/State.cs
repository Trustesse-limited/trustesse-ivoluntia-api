using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Trustesse.Ivoluntia.Domain.Entities;

public class State
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    [Required, StringLength(100)]
    public string StateName { get; set; }
    public string CountryId { get; set; }
    [ForeignKey("CountryId")]
    public virtual Country Country { get; set; }
}