using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Trustesse.Ivoluntia.Commons.DTOs.Foundation
{
    public class VerifyBankAccountResponseDto
    {
        [JsonPropertyName("responseCode")]
        public int ResponseCode { get; set; }

        [JsonPropertyName("isSuccessfull")]
        public bool IsSuccessful { get; set; }

        public string Message { get; set; }

        public List<string> Errors { get; set; }

        public OrganizationAccountNumberVerifyResponseDto Data { get; set; }
    }
}
