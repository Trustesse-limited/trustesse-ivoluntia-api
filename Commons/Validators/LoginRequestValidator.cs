using System;
using FluentValidation;
using Trustesse.Ivoluntia.Commons.DTOs.Auth;

namespace Trustesse.Ivoluntia.Commons.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequestModel>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(256).WithMessage("Email cannot exceed 256 characters")
            .Must(email => email == email?.ToLowerInvariant()).WithMessage("Email must be lowercase");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(1).WithMessage("Password cannot be empty")
            .MaximumLength(128).WithMessage("Password is too long");

        RuleFor(x => x.TwoFactorCode)
            .Length(6).WithMessage("Two-factor code must be 6 digits")
            .Matches(@"^\d{6}$").WithMessage("Two-factor code must contain only digits")
            .When(x => !string.IsNullOrEmpty(x.TwoFactorCode));

        RuleFor(x => x.DeviceInfo)
            .MaximumLength(500).WithMessage("Device info cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.DeviceInfo));
    }
}