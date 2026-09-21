using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.Security.AccessControl;

namespace Trustesse.Ivoluntia.Commons.DTOs.Auth;

public class LoginRequestModel
{
    public string Email { get; set; } 
    public string Password { get; set; }
    public bool RememberMe { get; set; } = false;
    public string? TwoFactorCode { get; set; }
    public string? DeviceInfo { get; set; }

    public LoginRequestModel Validate()
    {
        if (this == null)
            throw new Exception("invalid request");
        return this;
    }
}


public class LoginResponseModel
{
    public string Message { get; set; } = string.Empty;
    public string AccountType { get; set; }
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public bool HasCompletedOnboarding { get; set; }
    public int LastCompletedPage { get; set; }
    public int NumberOfPageRemaining { get; set; }     
    public DateTime AccessTokenExpireMinutes { get; set; } 
    public DateTime RefreshTokenExpireDays { get; set; }
    public bool HasSetUpPin { get; set; }
    public UserProfileSummary? UserProfile { get; set; }
    //public bool RequiresTwoFactor { get; init; }
    //public bool RequiresPasswordChange { get; init; }
    //public List<string> Permissions { get; init; } = new();    
}
public class PageCompletedUserData
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string OtherName { get; set; }
    public string Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string ZipCode { get; set; }
    public string Country { get; set; }
    public string State { get; set; }
    public List<string> InterestNames { get; set; }
    public List<string> SkillNames { get; set; }
    public string Bio { get; set; }
    public string ProfileImage { get; set; }
}
public class PageCompletedOrganizationData
{
    public string Name { get; set; }
    public string FoundationCategory { get; set; }
    public string Website { get; set; }
    public string Mission { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string Zipcode { get; set; }
    public string FoundationCountry { get; set; }
    public string FoundationState { get; set; }
    public List<string> CauseNames { get; set; } 
    public string Logo { get; set; }
    public bool HasAgreedToDisclaimer { get; set; }
}
