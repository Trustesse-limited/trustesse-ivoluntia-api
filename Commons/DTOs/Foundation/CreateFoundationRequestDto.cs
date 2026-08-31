using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Trustesse.Ivoluntia.Commons.DTOs.Foundation
{
    public class CreateFoundationRequestDto
    {
        public FoundationAdminInfo? FoundationAdminInfo { get; set; }
      
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
    
}
