using System.ComponentModel.DataAnnotations;

namespace Trustesse.Ivoluntia.Domain.Entities;

public class Country: BaseEntity
{
    public string CountryName { get; set; }
    public string CountryCode { get; set; }
    public ICollection<State> States { get; set; }
}