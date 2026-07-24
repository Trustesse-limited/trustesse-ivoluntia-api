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
        public string OtpCode { get; set; }
    }
}
