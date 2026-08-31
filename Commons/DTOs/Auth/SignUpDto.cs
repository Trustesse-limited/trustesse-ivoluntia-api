using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Trustesse.Ivoluntia.Commons.DTOs;

public class SignUpDto
{
    [EmailAddress(ErrorMessage = "Invalid Email Address")]
    public string Email { get; set; }
    public string Password { get; set; }
    [Compare("Password", ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; }
    [Required]
    [AllowedValues(true, ErrorMessage = "You must agree to the Terms and Conditions.")]
    public bool HasAgreedToTermsAndCondition { get; set; }
    public SignUpDto Validate()
    {
        if (this == null)
            throw new Exception("invalid request");
        return this;
    }
}


