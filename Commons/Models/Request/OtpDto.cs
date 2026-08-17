using Trustesse.Ivoluntia.Domain.Enums;

namespace Trustesse.Ivoluntia.Commons.Models.Request
{
    public  class GenerateOtpDto
    {
        public OtpPurpose Purpose { get; set; }
        public bool IncludeAlphabet { get; set; }
        public string Channel { get; set; } 
    }

    public class ConfirmOtpDto
    {
        public OtpPurpose Purpose { get; set; }
        public string OtpCode { get; set; }
    }
}
