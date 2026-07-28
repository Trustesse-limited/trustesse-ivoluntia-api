namespace Trustesse.Ivoluntia.Commons.Configurations;

public class TransactionSecurityOptions
{
    public string TokenSigningKey { get; set; }
    public int TokenExpiryMinutes { get; set; } = 2;
    public int MaxFailedPinAttempts { get; set; } = 5;
    public int LockoutMinutes { get; set; } = 15;
}
