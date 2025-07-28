using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Trustesse.Ivoluntia.Commons.DTOs.RSA
{
    public class RsaParameterResponse :BaseResponse
    {
        public RSAParameters Parameters { get; set; }
       
    }
}
