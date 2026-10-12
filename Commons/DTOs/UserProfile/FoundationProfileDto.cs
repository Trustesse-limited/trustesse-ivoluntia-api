using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trustesse.Ivoluntia.Commons.DTOs.UserProfile
{
    public class FoundationProfileDto
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
        public List<string>? Causes { get; set; }
        public string? LogoUrl { get; set; }
        public bool HasAgreedToDisclaimer { get; set; }
    }
}
