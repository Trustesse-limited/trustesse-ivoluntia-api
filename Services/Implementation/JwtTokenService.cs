using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Trustesse.Ivoluntia.Commons.Configurations;
using Trustesse.Ivoluntia.Commons.Contants;
using Trustesse.Ivoluntia.Commons.DTOs.Auth;
using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Domain.IRepositories;
using Trustesse.Ivoluntia.Services.Abstractions;

namespace Trustesse.Ivoluntia.Services.Implementation;

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _jwtOptions;
    private readonly ILogger<JwtTokenService> _logger;
    private readonly UserManager<User> _userManager;
    private readonly IUnitOfWork _unitOfWork;

    public JwtTokenService(
    IOptions<JwtOptions> jwtOptions,
    ILogger<JwtTokenService> logger,
    UserManager<User> userManager,
    IUnitOfWork unitOfWork)
    {
        _jwtOptions = jwtOptions.Value;
        _logger = logger;
        _userManager = userManager;
        _unitOfWork = unitOfWork;

    }

    public string GenerateAccessTokenAsync(JwtClaimsModel claims, string role)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtOptions.Key);

        // Get token expiration based on role
        var expirationMinutes = AuthenticationConstants.TokenExpirations.ContainsKey(role)
            ? AuthenticationConstants.TokenExpirations[role].AccessToken
            : 60;

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                    new Claim(ClaimTypes.Role, claims.Role),
                    new Claim(ClaimTypes.GivenName, claims.FirstName),
                    new Claim(ClaimTypes.Surname, claims.LastName),
                    new Claim("OrganizationName", claims.OrganizationName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
                }),
            Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
            Issuer = _jwtOptions.Issuer,
            Audience = _jwtOptions.Audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        //var tokenDescriptor = new SecurityTokenDescriptor
        //{
        //    Subject = new ClaimsIdentity(claims),
        //    Expires = DateTime.Now.AddDays(1),
        //    Audience = _jwtConfiguration.ValidAudience,
        //    Issuer = _jwtConfiguration.ValidIssuer,
        //    SigningCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256Signature)
        //};
        //var tokenHandler = new JwtSecurityTokenHandler();











        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    //public static string GenerateToken(User user, List<string> userRoles, string userPermissions, string lastLogin, List<Claim> appPermission)
    //{
    //    var appPermissionAsString = "";
    //    JwtConfiguration _jwtConfiguration = new();
    //    _configuration.Bind(_jwtConfiguration.SectionName, _jwtConfiguration);

    //    var orgCode = string.IsNullOrEmpty(user.Organisation.OrganisationCode) ? "" : user.Organisation.OrganisationCode;

    //    var claims = new List<Claim>
    //        {
    //            new Claim(CustomClaimTypes.Id, user.Id),
    //            new Claim(ClaimTypes.Email, user.Email),
    //            new Claim(ClaimTypes.Name, user.UserName),
    //            new Claim(CustomClaimTypes.OrganisationId, user.OrganisationId),
    //            new Claim(CustomClaimTypes.OrganisationCode, orgCode),
    //            new Claim(CustomClaimTypes.FirstName, user.LastName),
    //            new Claim(CustomClaimTypes.FirstName, user.FirstName),
    //            new Claim(CustomClaimTypes.UserOrganisationName, user.Organisation.OrganisationName),
    //            new Claim(CustomClaimTypes.Permissions, userPermissions),
    //            new Claim(CustomClaimTypes.UserLastLogin, lastLogin),
    //            new Claim(CustomClaimTypes.ProfilePicture, user.ProfilePicture),
    //            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    //        };


    //    foreach (var item in appPermission)
    //    {
    //        claims.Add(new Claim(CustomClaimTypes.AppPermissions, item.Value));
    //    }

    //    foreach (var role in userRoles)
    //    {
    //        claims.Add(new Claim(ClaimTypes.Role, role));
    //    }

    //    var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfiguration.Key));

    //    var tokenDescriptor = new SecurityTokenDescriptor
    //    {
    //        Subject = new ClaimsIdentity(claims),
    //        Expires = DateTime.Now.AddDays(1),
    //        Audience = _jwtConfiguration.ValidAudience,
    //        Issuer = _jwtConfiguration.ValidIssuer,
    //        SigningCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256Signature)
    //    };
    //    var tokenHandler = new JwtSecurityTokenHandler();
    //    var token = tokenHandler.CreateToken(tokenDescriptor);

    //    return tokenHandler.WriteToken(token);
    //}
    //acf-qqjo-bko

    public async Task<string> GenerateRefreshTokenAsync(string userId, string role)
    {
        //TODO: use custom exception
        var user = await _userManager.FindByIdAsync(userId) ?? throw new Exception("User not found");
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);

        var refreshToken = Convert.ToBase64String(randomNumber);


        var expirationMinutes = AuthenticationConstants.TokenExpirations.ContainsKey(role)
            ? AuthenticationConstants.TokenExpirations[role].RefreshToken
            : 1440;

        var userRefreshToken = new UserRefreshToken
        {
            Token = refreshToken,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes),
            CreatedBy = userId
        };

        _unitOfWork.refreshTokenRepo.Add(userRefreshToken);
        await _unitOfWork.CompleteAsync();

        _logger.LogInformation("Generated refresh token for user {UserId} with {ExpirationMinutes} minutes expiration",
            userId, expirationMinutes);

        return refreshToken;
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

        await _unitOfWork.CompleteAsync();

        _logger.LogInformation("Refresh token {TokenId} revoked by {RevokedBy} for user {UserId}. Reason: {Reason}",
            userRefreshToken.Id, revokedBy ?? "System", userRefreshToken.UserId, reason ?? "Manual revocation");


        return true;

    }

    public async Task<string?> RotateRefreshTokenAsync(string oldRefreshToken, string userId, string userRole)
    {
        var validation = await ValidateRefreshTokenAsync(oldRefreshToken, userId);

        if (!validation.IsValid || validation.UserId == null)
        {
            return null;
        }

        // Revoke the old token
        await RevokeRefreshTokenAsync(userId, oldRefreshToken, "System", "Token rotation");

        // Generate new token
        var newToken = await GenerateRefreshTokenAsync(
            validation.UserId, userRole);

        // Update the old token to reference the new token
        var oldTokenEntity = await _unitOfWork.refreshTokenRepo.GetUserRefreshTokenAsync(oldRefreshToken, userId);

        if (oldTokenEntity != null)
        {
            oldTokenEntity.ReplacedByToken = newToken;
            await _unitOfWork.CompleteAsync();
        }

        _logger.LogInformation("Rotated refresh token for user {UserId}", validation.UserId);
        return newToken;
    }

    public async Task<RefreshTokenValidationResult> ValidateRefreshTokenAsync(string refreshToken, string userId)
    {
        if (string.IsNullOrEmpty(refreshToken))
        {
            return new RefreshTokenValidationResult
            {
                IsValid = false,
                Status = RefreshTokenStatus.NotFound,
                ValidationError = "Refresh token is required"
            };
        }

        var userRefreshToken = await _unitOfWork.refreshTokenRepo.GetUserRefreshTokenAsync(refreshToken, userId);

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
            _logger.LogWarning("Attempted use of revoked refresh token {TokenId} by user {UserId}",
                userRefreshToken.Id, userRefreshToken.UserId);

            // Potential token theft - revoke all tokens for this user
            await RevokeAllUserRefreshTokensAsync(userRefreshToken.UserId, "System", "Potential token theft detected");

            return new RefreshTokenValidationResult
            {
                IsValid = false,
                Status = RefreshTokenStatus.Revoked,
                ValidationError = "Refresh token has been revoked"
            };
        }

        // Check if token is expired
        if (userRefreshToken.IsExpired)
        {
            _logger.LogInformation("Expired refresh token used by user {UserId}", userRefreshToken.UserId);
            return new RefreshTokenValidationResult
            {
                IsValid = false,
                Status = RefreshTokenStatus.Expired,
                ValidationError = "Refresh token has expired"
            };
        }


        _logger.LogDebug("Refresh token validated successfully for user {UserId}", userRefreshToken.UserId);

        return new RefreshTokenValidationResult
        {
            IsValid = true,
            Status = RefreshTokenStatus.Valid,
            UserId = userRefreshToken.UserId,
            ExpiresAt = userRefreshToken.ExpiresAt
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
            return principal;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token validation failed");
            return null;
        }

    }
}
