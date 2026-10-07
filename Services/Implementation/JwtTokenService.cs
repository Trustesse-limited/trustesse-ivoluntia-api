using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Trustesse.Ivoluntia.Commons.Configurations;
using Trustesse.Ivoluntia.Commons.Contants;
using Trustesse.Ivoluntia.Commons.DTOs.Auth;
using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Services.Abstractions;

namespace Trustesse.Ivoluntia.Services.Implementation;

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _jwtOptions;
    private readonly ILogger<JwtTokenService> _logger;
    private readonly UserManager<User> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDataProtectionProvider _dataProtectionProvider;

    public JwtTokenService(
    IOptions<JwtOptions> jwtOptions,
    ILogger<JwtTokenService> logger,
    UserManager<User> userManager,
    IUnitOfWork unitOfWork,
    IDataProtectionProvider dataProtectionProvider)
    {
        _jwtOptions = jwtOptions.Value;
        _logger = logger;
        _userManager = userManager;
        _unitOfWork = unitOfWork;
        _dataProtectionProvider = dataProtectionProvider;
    }
    public string GenerateAccessTokenAsync(JwtClaimsModel claims, string role)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtOptions.Key);
        var expirationMinutes = AuthenticationConstants.TokenExpirations.ContainsKey(role)
            ? AuthenticationConstants.TokenExpirations[role].AccessToken
            : 15;
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                    new Claim(JwtRegisteredClaimNames.Sub, claims.UserId),
                    new Claim(ClaimTypes.NameIdentifier, claims.UserId),
                    new Claim(JwtRegisteredClaimNames.Email, claims.Email),
                    new Claim(ClaimTypes.Name, claims.Email),
                    new Claim(ClaimTypes.Role, claims.Role),
                    new Claim(ClaimTypes.GivenName, claims.FirstName ?? string.Empty),
                    new Claim(ClaimTypes.Surname, claims.LastName ?? string.Empty),
                    new Claim("OrganizationName", claims.OrganizationName ?? string.Empty),
                    new Claim("FoundationId", claims.FoundationId ?? string.Empty),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
                }),
            Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
            Issuer = _jwtOptions.Issuer,
            Audience = _jwtOptions.Audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var handler = tokenHandler.CreateToken(tokenDescriptor);
        var token = tokenHandler.WriteToken(handler);
        var protect = _dataProtectionProvider.CreateProtector("JWTProtector");
        var encryptToken = protect.Protect(token);
        return encryptToken;
    }
    public async Task<string> GenerateRefreshTokenAsync(string userId, string role)
    {
        //TODO: use custom exception
        var user = await _userManager.FindByIdAsync(userId) ?? throw new Exception("User not found");
        var refresh = await _unitOfWork.refreshTokenRepo.GetByExpressionAsync(r => r.UserId == user.Id);
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        var refreshToken = Convert.ToBase64String(randomNumber);
        var protect = _dataProtectionProvider.CreateProtector("JWTProtector");
        var encryptRefreshToken = protect.Protect(refreshToken);
        var expirationDays = AuthenticationConstants.TokenExpirations.ContainsKey(role)
            ? AuthenticationConstants.TokenExpirations[role].RefreshToken
            : 30;
        if(refresh != null)
        {
            refresh.Token = encryptRefreshToken;
            refresh.ExpiresAt = DateTime.UtcNow.AddDays(expirationDays);
            refresh.CreatedAt = DateTime.UtcNow;
            _unitOfWork.refreshTokenRepo.Update(refresh);
            await _unitOfWork.CompleteAsync();
        }
        if(refresh == null)
        {
            var userRefreshToken = new UserRefreshToken
            {
                Token = encryptRefreshToken,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(expirationDays),
                CreatedBy = userId
            };
            _unitOfWork.refreshTokenRepo.Add(userRefreshToken);
            await _unitOfWork.CompleteAsync();
        }
        _logger.LogInformation($"Generated refresh token for user with id {user.Id}",
            userId, expirationDays);
        return encryptRefreshToken;
    }
    public async Task<string> UpdateRefreshTokenAsync(string userId, string refreshTokens)
    {
        //TODO: use custom exception
        var token =  await _unitOfWork.refreshTokenRepo.GetByExpressionIncludeAsync(r => r.Token == refreshTokens, r => r.User);
        if(token != null)
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            var refreshToken = Convert.ToBase64String(randomNumber);
            var expirationDays = 30;

            var userRefreshToken = new UserRefreshToken
            {
                Token = refreshToken,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(expirationDays),
                CreatedBy = userId
            };
            _unitOfWork.refreshTokenRepo.Update(userRefreshToken);
            await _unitOfWork.CompleteAsync();
            _logger.LogInformation($"Generated refresh token for user with id {userRefreshToken.User.Id}",
                userId, expirationDays);
            return token.Token;
        };
        return "refresh token not found";
    }
    public async Task<bool> RevokeAllUserRefreshTokensAsync(string userId, string? revokedBy = null, string? reason = null)
    {
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("Attempted to revoke tokens for null or empty user ID");
            return false;
        }
        var activeTokens = await _unitOfWork.refreshTokenRepo.GetActiveUserTokensAsync(userId);
        if (activeTokens == null)
        {
            _logger.LogInformation("No active refresh tokens found for user {UserId}", userId);
            return true;
        }
        var revokedCount = await _unitOfWork.refreshTokenRepo.BulkUpdateAsync(userId);
        _logger.LogInformation("Revoked {Count} refresh tokens for user {UserId} by {RevokedBy}. Reason: {Reason}",
            revokedCount, userId, revokedBy ?? "System", reason ?? "Revoke all user tokens");
        return true;
    }
    public async Task<bool> RevokeRefreshTokenAsync(string userId, string refreshToken, string revokedBy, string reason)
    {
        if (string.IsNullOrEmpty(refreshToken))
        {
            _logger.LogWarning("Attempted to revoke null or empty refresh token");
            return false;
        }
        var userRefreshToken = await _unitOfWork.refreshTokenRepo.GetUserRefreshTokenAsync(refreshToken, userId);
        if (userRefreshToken == null)
        {
            _logger.LogWarning("Attempted to revoke non-existent refresh token");
            return false;
        }
        if (userRefreshToken.IsRevoked)
        {
            _logger.LogInformation("Refresh token {TokenId} is already revoked", userRefreshToken.Id);
            return true;
        }
        userRefreshToken.IsRevoked = true;
        userRefreshToken.RevokedAt = DateTime.UtcNow;
        userRefreshToken.RevokedBy = revokedBy ?? "System";
        userRefreshToken.RevokedReason = reason ?? "Manual revocation";
        _unitOfWork.refreshTokenRepo.Update(userRefreshToken);       
        await _unitOfWork.CompleteAsync();

        _logger.LogInformation("Refresh token {TokenId} revoked by {RevokedBy} for user {UserId}. Reason: {Reason}",
            userRefreshToken.Id, revokedBy ?? "System", userRefreshToken.UserId, reason ?? "Manual revocation");
        return true;
    }
    public async Task<string?> RotateRefreshTokenAsync(string oldRefreshToken, string userId, string userRole, User user)
    {
        // Revoke the old token
        await RevokeRefreshTokenAsync(userId, oldRefreshToken, "System", "Token rotation");
        // Generate new token
        var newToken = await GenerateRefreshTokenAsync(
            user.Id, userRole);
        // Update the old token to reference the new token
        var oldTokenEntity = await _unitOfWork.refreshTokenRepo.GetUserRefreshTokenAsync(oldRefreshToken, userId);
        if (oldTokenEntity != null)
        {
            oldTokenEntity.ReplacedByToken = newToken;
            await _unitOfWork.CompleteAsync();
        }
        _logger.LogInformation("Rotated refresh token for user {UserId}", user.Id);
        return newToken;
    }
    public async Task<RefreshTokenValidationResult> ValidateRefreshTokenAsync(string refreshToken, string email)
    {
        var userRefreshToken = await _unitOfWork.refreshTokenRepo.GetByExpressionIncludeAsync(r => r.Token == refreshToken, r => r.User, r => r.User.Foundation);

        if (userRefreshToken == null)
        {
            _logger.LogWarning("Refresh token not found: {Token}", refreshToken.Substring(0, Math.Min(10, refreshToken.Length)) + "...");
            return new RefreshTokenValidationResult
            {
                IsValid = false,
                Status = RefreshTokenStatus.NotFound,
                ValidationError = "Invalid refresh token"
            };
        }
        if (userRefreshToken.IsRevoked)
        {
            _logger.LogWarning($"Attempted use of revoked refresh token {userRefreshToken.Id} by user {userRefreshToken.UserId}");
            // Potential token theft - revoke all tokens for this user
            await RevokeAllUserRefreshTokensAsync(userRefreshToken.UserId, "System", "Potential token theft detected");

            return new RefreshTokenValidationResult
            {
                IsValid = false,
                Status = RefreshTokenStatus.Revoked,
                ValidationError = "Refreh token expired"
            };
        }
        if (userRefreshToken.Token == refreshToken && userRefreshToken.ExpiresAt <= DateTime.Now)
        {  
            _logger.LogInformation("Expired refresh token used by user {UserId}", userRefreshToken.UserId);
            return new RefreshTokenValidationResult
            {
                IsValid = false,
                Status = RefreshTokenStatus.Expired
            };
        }
        _logger.LogDebug("Refresh token validated successfully for user {UserId}", userRefreshToken.UserId);
        return new RefreshTokenValidationResult
        {
            IsValid = true,
            Status = RefreshTokenStatus.Valid,
            UserId = userRefreshToken.UserId,
            ExpiresAt = userRefreshToken.ExpiresAt,
            User = userRefreshToken.User
        };
    }
    public ClaimsPrincipal ValidateTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtOptions.Key);
            var validationParameters = new TokenValidationParameters
            {   
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _jwtOptions.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtOptions.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
            var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
            var principals = tokenHandler.TokenLifetimeInMinutes;
            return principal;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token validation failed");
            return null;
        }
    }
    public ClaimsPrincipal GetPrincipalFromExpiredToken(string accessToken)
    {
        var key = Encoding.UTF8.GetBytes(_jwtOptions.Key);
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateLifetime = true 
        };
        var tokenHandler = new JwtSecurityTokenHandler();
        var protect = _dataProtectionProvider.CreateProtector("JWTProtector");
        var decrpytToken = protect.Unprotect(accessToken);
        var principal = tokenHandler.ValidateToken(decrpytToken, tokenValidationParameters, out SecurityToken securityToken);
        return principal;
    }
}
