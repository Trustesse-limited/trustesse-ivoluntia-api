using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trustesse.Ivoluntia.Commons.DTOs.Auth;

namespace Trustesse.Ivoluntia.Commons.Extensions.Helpers
{
    public class VerifyTwoFactorAuthenticationResponseDtoBuilder
    {
        public static VerifyTwoFactorAuthenticationResponseDto VerifyTwoFactorResponseBuilder(string AccessToken, string RefreshToken, string AccountType)
        {
            var verifyTwoFactorResponse = new VerifyTwoFactorAuthenticationResponseDto
            {
                AccessToken = AccessToken,
                RefreshToken = RefreshToken,    
                AccountType = AccountType   
            };
            return verifyTwoFactorResponse;
        }
    }
}
