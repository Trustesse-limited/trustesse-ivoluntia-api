using Trustesse.Ivoluntia.Domain.Enums;

namespace Trustesse.Ivoluntia.Commons.Models.Request
{
    public  class GenerateOtpDto
    {
        public string UserId { get; set; } = string.Empty;
        public OtpPurpose Purpose { get; set; }
    }

    public class ConfirmOtpDto
    {
        public string UserId { get; set; }
        public OtpPurpose Purpose { get; set; }
        public string OtpCode { get; set; }
    }
}
