using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trustesse.Ivoluntia.Domain.Enums;

namespace Trustesse.Ivoluntia.Commons.DTOs.Auth
{
    public class OtpDto
    {
       
        public string OtpCode { get; set; }
        public bool IsUsed { get; set; }
        public string UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string Purpose { get; set; }
        public string Channel { get; set; }
    }
}
