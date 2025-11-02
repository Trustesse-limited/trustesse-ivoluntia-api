using System;

namespace Trustesse.Ivoluntia.Commons.Configurations;

public class IdentityConfiguration
{
    public LockoutOptions Lockout { get; set; } = new();
    public PasswordOptions Password { get; set; } = new();
    public SignInOptions SignIn { get; set; } = new();
    public UserOptions User { get; set; } = new();
}

public class LockoutOptions
{
    public int DefaultLockoutTimeSpanMinutes { get; set; } = 60;
    public int MaxFailedAccessAttempts { get; set; } = 5;
    public bool AllowedForNewUsers { get; set; } = true;
}

public class PasswordOptions
{
    public bool RequireDigit { get; set; } = true;
    public int RequiredLength { get; set; } = 6;
    public bool RequireNonAlphanumeric { get; set; } = false;
    public bool RequireUppercase { get; set; } = true;
    public bool RequireLowercase { get; set; } = true;
}

public class SignInOptions
{
    public bool RequireConfirmedEmail { get; set; } = false;
    public bool RequireConfirmedPhoneNumber { get; set; } = false;
}

public class UserOptions
{
    public bool RequireUniqueEmail { get; set; } = true;
}
