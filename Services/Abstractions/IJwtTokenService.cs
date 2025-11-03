using System;
using System.Security.Claims;
using Trustesse.Ivoluntia.Commons.DTOs.Auth;

namespace Trustesse.Ivoluntia.Services.Abstractions;

public interface IJwtTokenService
{
    string GenerateAccessTokenAsync(JwtClaimsModel claims, string role);
    Task<string> GenerateRefreshTokenAsync(string userId, string role);
    ClaimsPrincipal ValidateTokenAsync(string token);
    Task<RefreshTokenValidationResult> ValidateRefreshTokenAsync(string refreshToken, string userId);
    Task<bool> RevokeAllUserRefreshTokensAsync(string userId, string? revokedBy = null, string? reason = null);
    Task<bool> RevokeRefreshTokenAsync(string userId, string refreshToken, string revokedBy, string reason);

    Task<string?> RotateRefreshTokenAsync(string oldRefreshToken, string userId, string userRole);
}