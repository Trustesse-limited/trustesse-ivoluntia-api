namespace Trustesse.Ivoluntia.Commons.DTOs.Auth;

public record AuthorizationTokenResult
{
    public bool IsValid { get; init; }
    public string? UserId { get; init; }
    public AuthorizationTokenStatus Status { get; init; }
}

public enum AuthorizationTokenStatus
{
    Valid,
    NotFound,
    Tampered,
    Expired,
    AlreadyUsed,
    WrongPurpose
}
