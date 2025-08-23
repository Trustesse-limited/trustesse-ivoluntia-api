using Microsoft.AspNetCore.Http;

namespace Trustesse.Ivoluntia.Commons.DTOs;

public class OnboardingMetaData
{
    public string  AccountType { get; set; }
    public int CurrentPage { get; set; }
}

public class AuthInfo
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string ConfirmPassword { get; set; }
    public bool HasAcceptedTOC { get; set; }
}

public class BioData
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int Gender { get; set; } 
    public DateTime DateOfBirth { get; set; }
}

public class Location
{
    public string Address { get; set; }
    public string City { get; set; }
    public string Country { get; set; }
    public string State { get; set; }
}

public class InterestDto
{
    public List<string> InterestId { get; set; }

    public InterestDto()
    {
        InterestId = new List<string>();
    }
}

public class Skill
{
    public List<string> SkillId { get; set; }

    public Skill()
    {
        SkillId = new List<string>();
    }
}

public class ProfileAndBioData
{
    public string Bio { get; set; }
    public IFormFile ProfileImage { get; set; }
}