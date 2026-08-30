using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trustesse.Ivoluntia.Commons.DTOs.Auth
{
    public class VerifyTwoFactorAuthenticationRequestDto
    {
        public string TwoFactorAuthCode { get; set; }
        public string Email { get; set; }       

        public VerifyTwoFactorAuthenticationRequestDto Validate()
        {
            if (this == null)
                throw new Exception("invalid request");
            return this;    
        }
    }
}
