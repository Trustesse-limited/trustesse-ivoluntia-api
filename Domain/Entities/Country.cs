using System.ComponentModel.DataAnnotations;

namespace Trustesse.Ivoluntia.Domain.Entities;

public class Country
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    [Required, StringLength(50)]
    public string CountryName { get; set; }
}