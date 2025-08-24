using System;

namespace Trustesse.Ivoluntia.Commons.DTOs.Auth;

public record JwtClaimsModel
{
    public string Role { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string OrganizationName { get; init; } = string.Empty;
    public DateTime IssuedAt { get; init; } = DateTime.UtcNow;
}
