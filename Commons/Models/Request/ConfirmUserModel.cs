using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trustesse.Ivoluntia.Domain.Enums;

namespace Trustesse.Ivoluntia.Commons.Models.Request
{
    public class ConfirmUserModel
    {
        public string Email { get; set; }
        public  string UserId { get; set; }
      public  string OtpCode { get; set; }
      public  OtpPurpose purpose { get; set; }
    }
}
