using System;
using FluentValidation;
using Trustesse.Ivoluntia.Commons.DTOs.Auth;

namespace Trustesse.Ivoluntia.Commons.Validators;

public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequestModel>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(x => x.AccessToken)
            .NotEmpty().WithMessage("Access token is required")
            .Must(BeValidJwtFormat).WithMessage("Invalid access token format");

        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required")
            .MinimumLength(32).WithMessage("Invalid refresh token format");

        RuleFor(x => x.UserId)
            .NotNull()
            .NotEmpty().WithMessage("UserId  is required");

    }

    private static bool BeValidJwtFormat(string token)
    {
        if (string.IsNullOrEmpty(token)) return false;
        var parts = token.Split('.');
        return parts.Length == 3;
    }
}
