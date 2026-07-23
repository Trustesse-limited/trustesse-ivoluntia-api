using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trustesse.Ivoluntia.Commons.DTOs
{
    public class VolunteerOnboardingResponse
    {
        public int NumberOfPageRemainingToComplete { get; set; }
        public bool OnboardingCompleted { get; set; }    
    }
}
