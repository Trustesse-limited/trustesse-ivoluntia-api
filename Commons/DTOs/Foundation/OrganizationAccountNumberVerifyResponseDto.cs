using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Trustesse.Ivoluntia.Commons.DTOs.Foundation
{
    public class OrganizationAccountNumberVerifyResponseDto
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public VerifyBankResponseData Data { get; set; }
    }
    public class VerifyBankResponseData
    {
        [JsonPropertyName("account_number")]
        public string AccountNumber { get; set; }
        [JsonPropertyName("account_name")]
        public string AccountName { get; set; }
    }
}
