using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Trustesse.Ivoluntia.Commons.Models.Response
{
    public class PaymentInitializeResponse
    {
        [JsonPropertyName("authorizationUrl")]
        public string AuthorizationUrl { get; set; }
        [JsonPropertyName("accesscode")]
        public string AccessCode { get; set; }
        public string Reference { get; set; }
    }
}
