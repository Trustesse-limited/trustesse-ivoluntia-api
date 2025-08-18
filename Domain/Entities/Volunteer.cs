using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Trustesse.Ivoluntia.Domain.Entities;

public class Volunteer : IdentityUser
{
        [Required]
        public string FirstName { get; set; }
        public string? MiddleName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public string Gender { get; set; }
        [Required]
        public string Address { get; set; }
        [Required]
        public override string UserName { get; set; }
        public string City { get; set; }
        [Required]
        public Guid StateId { get; set; }
        [Required]
        public Guid CountryId { get; set; }
        public string AccountType { get; set; }
        [Required]
        public bool IsActive { get; set; } = false;
        public DateTime? LastLogin { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? TokenExpiresBy { get; set; }
        [StringLength(6)]
        public string? OTP { get; set; }
        [StringLength(6)]
        public string? ForgotPasswordOTP { get; set; }
        public DateTime? ForgotPasswordOTPSubmitedTime { get; set; }      
        public DateTime? OtpSubmittedTime { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
}