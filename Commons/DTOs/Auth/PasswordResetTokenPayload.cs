using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trustesse.Ivoluntia.Commons.DTOs.Auth
{
    public class PasswordResetTokenPayload
    {
        public string Email { get; set; }   
        public string Otp { get; set; }
        public string Text { get; set; }    
    }
}
