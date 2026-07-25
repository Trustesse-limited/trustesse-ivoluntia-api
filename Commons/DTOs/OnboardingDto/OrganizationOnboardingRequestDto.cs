using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trustesse.Ivoluntia.Commons.DTOs.OnboardingDto
{
    public class OrganizationOnboardingRequestDto
    {
        public FoundationOnboardingMetaData MetaData { get; set; }
        public FoundationBioData? foundationBioData { get; set; }
        public FoundationLocationDto? FoundationLocationDto { get; set; }
        public CauseDto? CauseDto { get; set; }
        public ProfileLogo? ProfileLogo { get; set; }
        public Disclaimer? Disclaimer { get; set; }

        public OrganizationOnboardingRequestDto Validate()
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
    
    public class FoundationBioData
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string FoundationCategory { get; set; }
        public string Website { get; set; }
        [Required]
        public string Mission { get; set; }
    }
    public class FoundationLocationDto
    {
        public string Address { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string Zipcode { get; set; }
        [Required]
        public string FoundationCountry { get; set; }
        [Required]
        public string FoundationState { get; set; }
        public string? CountryId { get; set; }
        public string? StateId { get; set; }
        public string UserId { get; set; }
    }
    public class CauseDto
    {
        public List<string> Names { get; set; } = new List<string>();
    }
    public class ProfileLogo
    {
        public List<IFormFile> Logo { get; set; }
    }
    public class Disclaimer
    {
        [Required]
        public bool HasAgreedToDisclaimer { get; set; }
    }
}
