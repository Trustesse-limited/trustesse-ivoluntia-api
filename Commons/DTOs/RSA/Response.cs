using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trustesse.Ivoluntia.Commons.DTOs.RSA
{
    public class Response : BaseResponse
    {
        public string? EncryptedData { get; set; }
        public string? DecryptedData { get; set; }
        public string? temp { get; set; }
    }
}
