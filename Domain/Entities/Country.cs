using System.ComponentModel.DataAnnotations;

namespace Trustesse.Ivoluntia.Domain.Entities;

public class Country
{
    public long Id { get; set; }
    [Required, StringLength(50)]
    public string CountryName { get; set; }
}