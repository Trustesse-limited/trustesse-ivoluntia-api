using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trustesse.Ivoluntia.Commons.DTOs.OnboardingDto
{
    public class VolunteerOnboardingRequestDto
    {
        public OnboardingMetaData onboardingMetaData { get; set; } 
        public BioData? BioData { get; set; }
        public LocationDto? LocationDto { get; set; }
        public InterestDto? Interest { get; set; }
        public VolunteerSkillDto? Skill { get; set; }
        public ProfileImageAndBio? ProfileAndBioData { get; set; }

        public VolunteerOnboardingRequestDto Validate()
        {
            if (this == null)
                throw new Exception("invalid request");
            return this;
        }
    }

    public class OnboardingMetaData
    {
        public string AccountType { get; set; }
        public int CurrentPage { get; set; }
    }
    public class BioData
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public byte Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
    }

    public class LocationDto
    {
        public string Address { get; set; }
        public string City { get; set; }
        public string ZipCode { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
    }

    public class InterestDto
    {
        public List<string> Names { get; set; }
    }

    public class VolunteerSkillDto
    {
        public List<string> Names { get; set; }
    }

    public class ProfileImageAndBio
    {
        public string Bio { get; set; }
        public List<IFormFile> ProfileImage { get; set; }
    }
}
