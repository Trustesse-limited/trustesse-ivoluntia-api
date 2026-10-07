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
        public string Name { get; set; }
        public string FoundationCategory { get; set; }
        public string Website { get; set; }
        public string Mission { get; set; }
    }
    public class FoundationLocationDto
    {
        public string? Address { get; set; }
        public string City { get; set; }
        public string Zipcode { get; set; }
        public string FoundationCountry { get; set; }
        public string FoundationState { get; set; }
    }
    public class CauseDto
    {
        public List<string> Names { get; set; } = new List<string>();
    }
    public class ProfileLogo
    {
        public string LogoUrl { get; set; }
    }
    public class Disclaimer
    {
        public bool HasAgreedToDisclaimer { get; set; }
    }
}
