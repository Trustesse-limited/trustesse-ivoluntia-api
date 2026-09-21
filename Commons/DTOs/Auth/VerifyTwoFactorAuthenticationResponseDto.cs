using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trustesse.Ivoluntia.Commons.DTOs.Auth
{
    public class VerifyTwoFactorAuthenticationResponseDto
    {
        public string AccessToken { get; set; } 
        public string RefreshToken { get; set; }  
        public string AccountType { get; set; } 
        public UserProfileSummary UserProfileSummary { get; set; }  
    }
}
