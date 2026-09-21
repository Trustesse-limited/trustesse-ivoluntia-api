using System;

namespace Trustesse.Ivoluntia.Commons.DTOs.Auth;

public class UserProfileSummary
{
    public string FirstName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;   
    public string LastName { get; set; } = string.Empty;
    public string OtherName { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? UserImage { get; set; }
    public string Role { get; set; } = string.Empty;
    public string? OrganizationName { get; set; }
    public string UserType { get; set; } = string.Empty; 
    public bool IsActive { get; set; }
    public bool HasTwoFactorEnabled { get; set; }
    public DateTime LastLogin { get; set; }
    public DateTime? DateOfBirth { get; set; } 
    public string Gender { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;  
    public string City { get; set; }   
    public string ZipCode { get; set; } = string.Empty; 
    public string Country { get; set; }  
    public string CountryName { get; set; } = string.Empty;    
    public string State { get; set; } = string.Empty;  
    public string StateName { get; set; } = string.Empty;  
    public List<string> InterestNames { get; set; }  
    public List<string> SkillNames { get; set; } 
    public string Bio { get; set; } = string.Empty; 
    public string ProfileImage { get; set; } = string.Empty;  
    public string Category { get; set; } = string.Empty; 
    public string Website { get; set; } = string.Empty;  
    public string Mission { get; set; } = string.Empty; 
    public string FoundationCountry { get; set; } = string.Empty;
    public string FoundationState { get; set; } = string.Empty;    
    public List<string> CauseNames { get; set; }
    public string FoundationLogoUrl { get; set; } = string.Empty;   
    public string VolunteerImageUrl { get; set; } = string.Empty;  
}
