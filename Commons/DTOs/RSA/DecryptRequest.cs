using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trustesse.Ivoluntia.Commons.DTOs.RSA
{
    public class DecryptRequest
    {
        public string? MyPrivateKeyPath { set; get; }
        public string? EncryptedData { set; get; }
    }
}
