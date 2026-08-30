namespace Trustesse.Ivoluntia.Commons.DTOs.Auth;

public record JwtClaimsModel
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? FirstName { get; init; } = string.Empty;
    public string? LastName { get; init; } = string.Empty;
    public string? OrganizationName { get; set; } = string.Empty;
    public string? FoundationId { get; set; } = string.Empty;
    public DateTime IssuedAt { get; init; } = DateTime.UtcNow;
}
