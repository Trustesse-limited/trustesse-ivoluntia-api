using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using Trustesse.Ivoluntia.Commons.Configurations;
using Trustesse.Ivoluntia.Commons.DTOs.Auth;
using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Domain.IRepositories;
using Trustesse.Ivoluntia.Services.BusinessLogics.IService;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.Service;

public class AuthorizationTokenService : IAuthorizationTokenService
{
    private const int ReferenceLength = 16;
    private const int SecretLength = 48;
    private const int SaltLength = 32;

    private readonly IUnitOfWork _uow;
    private readonly TransactionSecurityOptions _options;

    public AuthorizationTokenService(IUnitOfWork uow, IOptions<TransactionSecurityOptions> options)
    {
        _uow = uow;
        _options = options.Value;
    }

    public async Task<string> GenerateTokenAsync(string userId, string purpose)
    {
        var reference = RandomNumberGenerator.GetBytes(ReferenceLength);
        var secret = RandomNumberGenerator.GetBytes(SecretLength);
        var salt = RandomNumberGenerator.GetBytes(SaltLength);

        var authorization = new Authorization
        {
            TokenHash = ComputeHash(secret, salt),
            TokenSalt = salt,
            ByteString = reference,
            IsUsed = false,
            InitiatorId = userId,
            Purpose = purpose,
            DateCreated = DateTime.UtcNow
        };

        await _uow.authorizationRepo.AddAsync(authorization);
        await _uow.CompleteAsync();

        var tokenBytes = new byte[ReferenceLength + SecretLength];
        Buffer.BlockCopy(reference, 0, tokenBytes, 0, ReferenceLength);
        Buffer.BlockCopy(secret, 0, tokenBytes, ReferenceLength, SecretLength);

        return Convert.ToBase64String(tokenBytes);
    }

    public async Task<AuthorizationTokenResult> VerifyTokenAsync(string token, string userId, string purpose)
    {
        byte[] tokenBytes;

        try
        {
            tokenBytes = Convert.FromBase64String(token);
        }
        catch (FormatException)
        {
            return Invalid(AuthorizationTokenStatus.Tampered);
        }

        if (tokenBytes.Length != ReferenceLength + SecretLength)
            return Invalid(AuthorizationTokenStatus.Tampered);

        var reference = tokenBytes[..ReferenceLength];
        var secret = tokenBytes[ReferenceLength..];

        var authorization = await _uow.authorizationRepo.GetByExpressionAsync(a => a.ByteString == reference);

        if (authorization == null || authorization.InitiatorId != userId)
            return Invalid(AuthorizationTokenStatus.NotFound);

        var expectedHash = ComputeHash(secret, authorization.TokenSalt);

        if (!CryptographicOperations.FixedTimeEquals(expectedHash, authorization.TokenHash))
            return Invalid(AuthorizationTokenStatus.Tampered);

        if (authorization.IsUsed)
            return Invalid(AuthorizationTokenStatus.AlreadyUsed);

        if (DateTime.UtcNow - authorization.DateCreated > TimeSpan.FromMinutes(_options.TokenExpiryMinutes))
            return Invalid(AuthorizationTokenStatus.Expired);

        if (authorization.Purpose != purpose)
            return Invalid(AuthorizationTokenStatus.WrongPurpose);

        return new AuthorizationTokenResult
        {
            IsValid = true,
            UserId = authorization.InitiatorId,
            Status = AuthorizationTokenStatus.Valid
        };
    }

    private byte[] ComputeHash(byte[] secret, byte[] salt)
    {
        var signingKey = Convert.FromBase64String(_options.TokenSigningKey);
        var key = new byte[signingKey.Length + salt.Length];
        Buffer.BlockCopy(signingKey, 0, key, 0, signingKey.Length);
        Buffer.BlockCopy(salt, 0, key, signingKey.Length, salt.Length);

        using var hmac = new HMACSHA512(key);
        return hmac.ComputeHash(secret);
    }

    private static AuthorizationTokenResult Invalid(AuthorizationTokenStatus status)
        => new() { IsValid = false, Status = status };
}
