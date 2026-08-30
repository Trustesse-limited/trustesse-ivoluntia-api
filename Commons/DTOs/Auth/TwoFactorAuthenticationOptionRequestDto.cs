using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trustesse.Ivoluntia.Commons.DTOs.Auth
{
     public class TwoFactorAuthenticationOptionRequestDto
    {
        public string Option { get; set; }  
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; } 

        public TwoFactorAuthenticationOptionRequestDto Validate()
        {
            if (this.Option == "Email" && Email == null)
                throw new Exception("user email is required");
            if (this.Option == "SMS" && PhoneNumber == null)
                throw new Exception("user phoneNumber is required");
            return this;    
        }
    }
}
