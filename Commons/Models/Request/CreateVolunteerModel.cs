using System.ComponentModel.DataAnnotations;

namespace Trustesse.Ivoluntia.Commons.Models.Request;

public class CreateVolunteerModel
{
    [Required]
    public required string FirstName { get; set; }
    public string? MiddleName { get; set; }
    [Required]
    public required string LastName { get; set; }
    [Required]
    public string MobilePhoneNumber { get; set; }
    [DataType(DataType.EmailAddress)]
    [Required]
    public string EmailAddress{ get; set; }
    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; }
    [Required]
    public string Gender { get; set; }
    [Required]
    public string Address { get; set; }
    [Required]
    public string City { get; set; }
    [Required]
    public Guid StateId { get; set; }
    [Required]
    public Guid CountryId { get; set; }
}