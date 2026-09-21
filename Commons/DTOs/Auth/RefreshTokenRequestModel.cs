using System;

namespace Trustesse.Ivoluntia.Commons.DTOs.Auth;

public record RefreshTokenRequestModel
{
    public string Email { get; set; }
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;

    public RefreshTokenRequestModel Validate()
    {
        if (this == null)
            throw new Exception("invalid request");
        return this;    
    }
}
public record RefreshTokenResponseModel
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public DateTime AccessTokenExpiresAt { get; init; }
    public DateTime RefreshTokenExpiresAt { get; init; }
}
