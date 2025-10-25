using System;
using System.ComponentModel.DataAnnotations;

namespace Trustesse.Ivoluntia.Domain.Entities;

public class UserRefreshToken : BaseEntity
{

    [Required]
    public string UserId { get; set; }

    [Required]
    public string Token { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsRevoked { get; set; } = false;

    public DateTime? RevokedAt { get; set; }

    public bool IsActive => !IsRevoked && DateTime.UtcNow <= ExpiresAt;

    public bool IsExpired => DateTime.UtcNow > ExpiresAt;
    public string? RevokedBy { get; set; }
    public string? RevokedReason { get; set; }
    public string? ReplacedByToken { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;
}
