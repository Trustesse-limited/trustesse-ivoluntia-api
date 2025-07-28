using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trustesse.Ivoluntia.Commons.DTOs.RSA
{
    public class EncryptRequest
    {
        public string? PartnerPublicKeyPath { get; set; }
        public string? TextInput { get; set; }
    }
}
