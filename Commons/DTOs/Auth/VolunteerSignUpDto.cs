using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Trustesse.Ivoluntia.Commons.DTOs;

public class VolunteerSignUpDto
{
   public OnboardingMetaData MetaData { get; set; }
   public AuthInfo? AuthInfo { get; set; }
   public BioData? BioData { get; set; }
   public LocationDto? LocationDto { get; set; }
   public InterestDto? Interest { get; set; }
   public SkillDto? Skill { get; set; }
   public ProfileImageAndBio? ProfileAndBioData { get; set; }
}

public class OnboardingMetaData
{
    public string  AccountType { get; set; }
    public int CurrentPage { get; set; }
}
public class AuthInfo
{
    [EmailAddress(ErrorMessage = "Invalid Email Address")]
    public string Email { get; set; }
    public string Password { get; set; }
    [Compare("Password", ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; }
    [Required]
    [AllowedValues(true, ErrorMessage = "You must agree to the Terms and Conditions.")]
    public bool HasAcceptedTOC { get; set; }
}

public class BioData
{
    public string UserId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public byte Gender { get; set; } 
    public DateTime DateOfBirth { get; set; }
}

public class LocationDto
{
    public string UserId { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string ZipCode { get; set; }
    public string CountryId { get; set; }
    public string StateId { get; set; }
}

public class InterestDto
{
    public string UserId { get; set; }
    public List<string> Names { get; set; }

    public InterestDto()
    {
        Names = new List<string>();
    }
}

public class SkillDto
{
    public string UserId { get; set; }
    public List<string> Names { get; set; }

    public SkillDto()
    {
        Names = new List<string>();
    }
}

public class ProfileImageAndBio
{
    public string UserId { get; set; }
    public string Bio { get; set; }
    public string ProfileImageurl { get; set; }
}