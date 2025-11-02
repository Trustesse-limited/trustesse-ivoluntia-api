using System;

namespace Trustesse.Ivoluntia.Commons.Contants;

public static class AuthenticationConstants
{
    public const int MAX_FAILED_ATTEMPTS = 5;
    public const int LOCKOUT_DURATION_HOURS = 1;
    public const int TWO_FACTOR_CODE_EXPIRY_MINUTES = 5;
    public const int PASSWORD_RESET_TOKEN_EXPIRY_HOURS = 24;
    public const int TWO_FACTOR_TOKEN_EXPIRY_MINUTES = 10;

    public static readonly Dictionary<string, TokenExpiration> TokenExpirations = new()
        {
            { "SuperAdmin", new TokenExpiration(AccessToken: 15, RefreshToken: 30) },
            { "FoundationAdmin", new TokenExpiration(AccessToken: 15, RefreshToken: 30) },
            { "Volunteer", new TokenExpiration(AccessToken: 120, RefreshToken: 10080) } // 2 hours, 1 week
        };

    public static readonly List<string> ValidGenders = new() { "Male", "Female", "Other" };
    public static readonly List<string> ValidAccountTypes = new() { "Volunteer", "Foundation" };

    // Security settings
    public const int MIN_PASSWORD_LENGTH = 8;
    public const int MAX_PASSWORD_LENGTH = 128;
    public const int MAX_LOGIN_ATTEMPTS_PER_HOUR = 10;
    public const int DEVICE_REMEMBER_DAYS = 30;

    public const string Volunteer = "Volunteer";
    public const string FoundationAdmin = "FoundationAdmin";
    public const string SuperAdmin = "SuperAdmin"; 
}

public record TokenExpiration(int AccessToken, int RefreshToken);