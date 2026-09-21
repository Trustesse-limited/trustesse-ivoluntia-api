

using Trustesse.Ivoluntia.Domain.Entities;

namespace Trustesse.Ivoluntia.Commons.DTOs.Auth;

public record RefreshTokenValidationResult
{
    public bool IsValid { get; init; }
    public string? UserId { get; init; }
    public string? UserRole { get; init; }
    public DateTime? ExpiresAt { get; init; }
    public string? ValidationError { get; init; }
    public RefreshTokenStatus Status { get; init; }
    public User User { get; set; } 
}

public enum RefreshTokenStatus
{
    Valid,
    NotFound,
    Expired,
    Revoked,
    UserNotFound,
    UserInactive,
    FoundationInactive
}
