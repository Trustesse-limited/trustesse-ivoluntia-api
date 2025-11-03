namespace Trustesse.Ivoluntia.Commons.DTOs.Auth;

public record LoginRequestModel
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public bool RememberMe { get; init; } = false;
    public string? TwoFactorCode { get; init; }
    public string? DeviceInfo { get; init; }
}


public record LoginResponseModel
{
    public string Message { get; init; } = string.Empty;
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public bool HasCompletedOnboarding { get; init; }
    public int LastCompletedPage { get; init; }
    public DateTime TokenExpiresAt { get; init; }
    public DateTime RefreshTokenExpiresAt { get; init; }
    public UserProfileSummary? UserProfile { get; init; }
    public bool RequiresTwoFactor { get; init; }
    public bool RequiresPasswordChange { get; init; }
    public List<string> Permissions { get; init; } = new();
}
