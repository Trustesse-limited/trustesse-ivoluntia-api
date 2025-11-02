using System;

namespace Trustesse.Ivoluntia.Commons.DTOs.Auth;

public record RefreshTokenRequestModel
{
    public string UserId { get; set; }
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
}

public record RefreshTokenResponseModel
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public DateTime TokenExpiresAt { get; init; }
    public DateTime RefreshTokenExpiresAt { get; init; }
}
