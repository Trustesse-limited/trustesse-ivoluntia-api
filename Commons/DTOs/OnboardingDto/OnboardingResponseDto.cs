using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trustesse.Ivoluntia.Commons.DTOs.OnboardingDto
{
    public class OnboardingResponseDto
    {
        public int NumberOfPageRemainingToComplete { get; set; }
        public bool OnboardingCompleted { get; set; }

        public static OnboardingResponseDto BuildOnboardingResponseDto(int numberOfPageRemainingToComplete, bool onboardingCompleted)
        {
            OnboardingResponseDto onboardingResponseDto = new OnboardingResponseDto
            {
                NumberOfPageRemainingToComplete = numberOfPageRemainingToComplete,  
                OnboardingCompleted = onboardingCompleted   
            };
            return onboardingResponseDto;   
        }
    }
}
