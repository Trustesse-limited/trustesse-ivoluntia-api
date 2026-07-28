namespace Trustesse.Ivoluntia.Commons.DTOs.Auth;

public class SetupTransactionPinRequest
{
    public string Pin { get; set; } = string.Empty;
    public string ConfirmPin { get; set; } = string.Empty;
}

public class SetupTransactionPinResponse
{
    public bool PinSetupComplete { get; set; }
}
