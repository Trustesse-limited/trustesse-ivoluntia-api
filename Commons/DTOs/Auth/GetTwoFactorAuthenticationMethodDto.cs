using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trustesse.Ivoluntia.Commons.DTOs.Auth
{
    public class GetTwoFactorAuthenticationMethodDto
    {
        public string Option { get; set; }
        public string Email { get; set; }   
        public string PhoneNumber { get; set; } 
    }
}
