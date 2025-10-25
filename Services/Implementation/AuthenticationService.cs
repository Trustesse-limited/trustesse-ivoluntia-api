using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Trustesse.Ivoluntia.Commons.Contants;
using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Commons.DTOs.Auth;
using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Domain.IRepositories;
using Trustesse.Ivoluntia.Services.Abstractions;

namespace Trustesse.Ivoluntia.Services.Implementation;

public class AuthenticationService(
           UserManager<User> userManager,
           IJwtTokenService jwtTokenService,
          IUserRepository userRepository,
          IUnitOfWork unitOfWork,
           ILogger<AuthenticationService> logger) : IAuthenticationService
{
    public async Task<ApiResponse<LoginResponseModel>> LoginAsync(LoginRequestModel request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetUserByEmailWithFoundationAsync(request.Email, cancellationToken);

        if (user is null)
        {
            return ApiResponse<LoginResponseModel>.Failure(401, "Invalid credentials");
        }

        if (!user.IsActive || user.Foundation?.IsActive != true)
        {
            return ApiResponse<LoginResponseModel>.Failure(401, "Account is inactive");
        }

        if (await userManager.IsLockedOutAsync(user))
        {
            return ApiResponse<LoginResponseModel>.Failure(403, "Account is locked for 1 hour due to multiple failed login attempts.");
        }

        if (!await userManager.CheckPasswordAsync(user, request.Password))
        {
            await userManager.AccessFailedAsync(user);
            return ApiResponse<LoginResponseModel>.Failure(401, "Invalid credentials");
        }

        await userManager.ResetAccessFailedCountAsync(user);

        user.LastLogin = DateTime.UtcNow;
        user.DateUpdated = DateTime.UtcNow;

        var roles = await userManager.GetRolesAsync(user);
        var primaryRole = roles.FirstOrDefault() ?? "Volunteer";

        var jwtClaims = new JwtClaimsModel
        {
            Role = primaryRole,
            FirstName = user.FirstName,
            LastName = user.LastName,
            OrganizationName = user?.Foundation?.Name!,
        };

        var accessToken = jwtTokenService.GenerateAccessTokenAsync(jwtClaims, primaryRole);
        var refreshToken = await jwtTokenService.GenerateRefreshTokenAsync(
            user?.Id!, primaryRole);

        user!.LastLogin = DateTime.UtcNow;

        await unitOfWork.CompleteAsync();

        var longinResponse = new LoginResponseModel
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            HasCompletedOnboarding = user.OnboardingProgress.HasCompletedOnboarding,
            LastCompletedPage = user.OnboardingProgress.LastCompletedPage,
            Message = "Login successful"
        };

        return ApiResponse<LoginResponseModel>.Success("Successfully logged in", longinResponse);
    }

    public async Task<ApiResponse<RefreshTokenResponseModel>> RefreshTokenAsync(RefreshTokenRequestModel request, CancellationToken cancellationToken)
    {
        var validation = await jwtTokenService.ValidateRefreshTokenAsync(request.RefreshToken, request.UserId);

        if (!validation.IsValid)
        {
            logger.LogWarning("Invalid refresh token used. Status: {Status}, Error: {Error}",
                validation.Status, validation.ValidationError);

            return ApiResponse<RefreshTokenResponseModel>.Failure(400, $"Invalid refresh token due to {nameof(validation.Status)}");
        }


        var user = await userRepository.GetUserByEmailWithFoundationAsync(request.UserId, cancellationToken);

        if (user is null)
        {
            return ApiResponse<RefreshTokenResponseModel>.Failure(404, "User not found");
        }

        var userRoles = await userManager.GetRolesAsync(user);
        var userRole = userRoles.First() ?? "Volunteer";

        var jwtClaims = new JwtClaimsModel
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            OrganizationName = user?.Foundation?.Name!,
        };

        var newRefreshToken = await jwtTokenService.RotateRefreshTokenAsync(request.RefreshToken, user.Id, userRole);


        if (string.IsNullOrEmpty(newRefreshToken))
        {
            logger.LogError("Failed to rotate refresh token for user {UserId}", request.UserId);
            return ApiResponse<RefreshTokenResponseModel>.Failure(400, "Failed to generate new refresh token");
        }

        var newAccessToken = jwtTokenService.GenerateAccessTokenAsync(jwtClaims, userRole);

        var tokenExpirations = AuthenticationConstants.TokenExpirations.ContainsKey(userRole)
                    ? AuthenticationConstants.TokenExpirations[userRole]
                    : new TokenExpiration(AccessToken: 60, RefreshToken: 1440); // Default values

        var accessTokenExpiresAt = DateTime.UtcNow.AddMinutes(tokenExpirations.AccessToken);
        var refreshTokenExpiresAt = DateTime.UtcNow.AddMinutes(tokenExpirations.RefreshToken);


        logger.LogInformation("Token refresh successful for user {UserId}. New tokens generated.", request.UserId);

        // Create response
        var refreshResponse = new RefreshTokenResponseModel
        {
            Success = true,
            Message = "Tokens refreshed successfully",
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            TokenExpiresAt = accessTokenExpiresAt,
            RefreshTokenExpiresAt = refreshTokenExpiresAt
        };

        return ApiResponse<RefreshTokenResponseModel>.Success("Tokens refreshed successfully", refreshResponse);


    }
}
