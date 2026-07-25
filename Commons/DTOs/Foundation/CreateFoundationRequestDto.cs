using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Trustesse.Ivoluntia.Commons.DTOs.Foundation
{
    public class CreateFoundationRequestDto
    {
        //public FoundationOnboardingMetaData MetaData { get; set; }
        public FoundationAdminInfo? FoundationAdminInfo { get; set; }
        //public FoundationBioData? foundationBioData { get; set; }
        //public FoundationLocationDto? FoundationLocationDto { get; set; }
        //public CauseDto? CauseDto { get; set; }
        //public ProfileLogo? ProfileLogo { get; set; }
        //public Disclaimer? Disclaimer { get; set; }  

        public CreateFoundationRequestDto Validate()
        {
            if (this == null)
                throw new Exception("invalid request");
            return this;
        }
    }
    public class FoundationOnboardingMetaData
    {
        public string AccountType { get; set; }
        public int CurrentPage { get; set; }
    }
    public class FoundationAdminInfo
    {
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }
        [Required]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,}$",ErrorMessage = "Password must be at least 8 characters long and contain at least one uppercase letter, one lowercase letter, one number, and one special character.")]
        public string Password { get; set; }
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }
        [Required]
        [AllowedValues(true, ErrorMessage = "You must agree to the Terms and Conditions.")]
        public bool HasAgreedToTermsAndCondition { get; set; }
    }
    //public class FoundationBioData
    //{
    //    [Required]
    //    public string Name { get; set; }
    //    [Required]
    //    public string FoundationCategory { get; set; }
    //    public string Website { get; set; }
    //    [Required]
    //    public string Mission { get; set; }
    //}
    //public class FoundationLocationDto
    //{
    //    public string Address { get; set; }
    //    [Required]
    //    public string City { get; set; }
    //    [Required]
    //    public string Zipcode { get; set; }
    //    [Required]
    //    public string FoundationCountry { get; set; }
    //    [Required]
    //    public string FoundationState { get; set; }
    //    public string? CountryId { get; set; }   
    //    public string? StateId { get; set; } 
    //    public string UserId { get; set; }  
    //}
    //public class CauseDto
    //{
    //    public List<string> Names { get; set; } = new List<string>();   
    //}
    //public class ProfileLogo
    //{
    //    public List<IFormFile> Logo { get; set; } 
    //}
    //public class Disclaimer
    //{
    //    [Required]
    //    public bool HasAgreedToDisclaimer { get; set; }
    //}
}
