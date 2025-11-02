using System;

namespace Trustesse.Ivoluntia.Commons.DTOs.Auth;

public record UserProfileSummary
{
    public string Id { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string Email { get; init; } = string.Empty;
    public string? UserImage { get; init; }
    public string Role { get; init; } = string.Empty;
    public string? OrganizationName { get; init; }
    public string UserType { get; init; } = string.Empty; 
    public bool IsActive { get; init; }
    public bool HasTwoFactorEnabled { get; init; }
    public DateTime LastLogin { get; init; }
}
