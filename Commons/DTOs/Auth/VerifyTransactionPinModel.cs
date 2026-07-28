using System.ComponentModel.DataAnnotations;

namespace Trustesse.Ivoluntia.Commons.DTOs.Auth;

public class VerifyTransactionPinRequest
{
    [Required(ErrorMessage = "PIN is required.")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "PIN must be exactly 6 digits.")]
    public string Pin { get; set; } = string.Empty;
}

public class VerifyTransactionPinResponse
{
    public string Token { get; set; } = string.Empty;
}
