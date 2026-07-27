using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Trustesse.Ivoluntia.Commons.Configurations;
using Trustesse.Ivoluntia.Commons.DTOs.Auth;
using Trustesse.Ivoluntia.Commons.Extensions.Helpers;
using Trustesse.Ivoluntia.Commons.Models.Response;
using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Domain.Enums;
using Trustesse.Ivoluntia.Domain.IRepositories;
using Trustesse.Ivoluntia.Services.BusinessLogics.Interfaces;
using Trustesse.Ivoluntia.Services.BusinessLogics.IService;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.Service;

public class TransactionPinService : ITransactionPinService
{
    private static readonly HashSet<string> WeakSequentialRuns = BuildSequentialRuns();

    private readonly ICurrentUserService _currentUserService;
    private readonly IPasswordHasher<string> _passwordHasher;
    private readonly IAuthorizationTokenService _authorizationTokenService;
    private readonly IUnitOfWork _uow;
    private readonly TransactionSecurityOptions _options;

    public TransactionPinService(
        ICurrentUserService currentUserService,
        IPasswordHasher<string> passwordHasher,
        IAuthorizationTokenService authorizationTokenService,
        IUnitOfWork uow,
        IOptions<TransactionSecurityOptions> options)
    {
        _currentUserService = currentUserService;
        _passwordHasher = passwordHasher;
        _authorizationTokenService = authorizationTokenService;
        _uow = uow;
        _options = options.Value;
    }

    public async Task<GlobalRequestReponse<SetupTransactionPinResponse>> SetupTransactionPinAsync(SetupTransactionPinRequest request)
    {
        try
        {
            var userId = _currentUserService.GetUserId();

            if (string.IsNullOrWhiteSpace(userId))
                return ResponseHelper.BuildResponse<SetupTransactionPinResponse>("Invalid user", StatusCodes.Status400BadRequest, null, false);

            var securityQuestions = await _uow.userSecurityQuestionRepo.GetListByExpressionAsync(x => x.UserId == userId);

            if (!securityQuestions.Any())
                return ResponseHelper.BuildResponse<SetupTransactionPinResponse>("You must set up your security questions before creating a transaction PIN.", StatusCodes.Status400BadRequest, null, false);

            var existingPin = await _uow.transactionPinRepo.GetByExpressionAsync(x => x.UserId == userId);

            if (existingPin != null)
                return ResponseHelper.BuildResponse<SetupTransactionPinResponse>("Transaction PIN has already been set.", StatusCodes.Status400BadRequest, null, false);

            if (!IsValidFormat(request.Pin))
                return ResponseHelper.BuildResponse<SetupTransactionPinResponse>("PIN must be exactly 6 digits.", StatusCodes.Status400BadRequest, null, false);

            if (request.Pin != request.ConfirmPin)
                return ResponseHelper.BuildResponse<SetupTransactionPinResponse>("PIN and confirmation do not match.", StatusCodes.Status400BadRequest, null, false);

            if (IsWeakPin(request.Pin))
                return ResponseHelper.BuildResponse<SetupTransactionPinResponse>("This PIN is too easy to guess. Please choose a different one.", StatusCodes.Status400BadRequest, null, false);

            var pin = new TransactionPin
            {
                UserId = userId,
                PinHash = _passwordHasher.HashPassword(userId, request.Pin),
                CreatedDate = DateTime.UtcNow
            };

            await _uow.transactionPinRepo.AddAsync(pin);
            await _uow.CompleteAsync();

            return ResponseHelper.BuildResponse("Transaction PIN created successfully.", StatusCodes.Status200OK,
                new SetupTransactionPinResponse { PinSetupComplete = true }, true);
        }
        catch (Exception ex)
        {
            return ResponseHelper.BuildResponse<SetupTransactionPinResponse>(ex.Message, StatusCodes.Status500InternalServerError, null, false);
        }
    }

    public async Task<GlobalRequestReponse<VerifyTransactionPinResponse>> VerifyTransactionPinAsync(VerifyTransactionPinRequest request)
    {
        try
        {
            var userId = _currentUserService.GetUserId();

            if (string.IsNullOrWhiteSpace(userId))
                return ResponseHelper.BuildResponse<VerifyTransactionPinResponse>("Invalid user.", StatusCodes.Status401Unauthorized, null, false);

            var now = DateTime.UtcNow;

            var attempt = await _uow.pinVerificationAttemptRepo.GetByExpressionAsync(x => x.UserId == userId);

            if (attempt?.LockedUntil > now)
                return ResponseHelper.BuildResponse<VerifyTransactionPinResponse>(
                    $"Too many failed attempts. Try again after {attempt.LockedUntil:yyyy-MM-dd HH:mm:ss} UTC.", StatusCodes.Status423Locked, null, false);

            if (attempt == null)
            {
                attempt = new PinVerificationAttempt
                {
                    UserId = userId,
                    AttemptCount = 0,
                    LastAttemptDate = now
                };

                await _uow.pinVerificationAttemptRepo.AddAsync(attempt);
            }

            var pin = await _uow.transactionPinRepo.GetByExpressionAsync(x => x.UserId == userId);

            if (pin == null)
                return ResponseHelper.BuildResponse<VerifyTransactionPinResponse>("Transaction PIN has not been set up.", StatusCodes.Status400BadRequest, null, false);

            var verificationResult = _passwordHasher.VerifyHashedPassword(userId, pin.PinHash, request.Pin);
            var isValid = verificationResult == PasswordVerificationResult.Success || verificationResult == PasswordVerificationResult.SuccessRehashNeeded;

            if (!isValid)
            {
                attempt.AttemptCount++;
                attempt.LastAttemptDate = now;

                if (attempt.AttemptCount >= _options.MaxFailedPinAttempts)
                    attempt.LockedUntil = now.AddMinutes(_options.LockoutMinutes);

                await _uow.CompleteAsync();

                var remainingAttempts = Math.Max(0, _options.MaxFailedPinAttempts - attempt.AttemptCount);

                return ResponseHelper.BuildResponse<VerifyTransactionPinResponse>(
                    $"Incorrect PIN. {remainingAttempts} attempt(s) remaining.", StatusCodes.Status400BadRequest, null, false);
            }

            attempt.AttemptCount = 0;
            attempt.LockedUntil = null;
            attempt.LastAttemptDate = now;

            var token = await _authorizationTokenService.GenerateTokenAsync(userId, AuthorizationPurpose.TransactionPinVerification.ToString());

            await _uow.CompleteAsync();

            return ResponseHelper.BuildResponse("Transaction PIN verified successfully.", StatusCodes.Status200OK,
                new VerifyTransactionPinResponse { Token = token }, true);
        }
        catch (Exception ex)
        {
            return ResponseHelper.BuildResponse<VerifyTransactionPinResponse>(ex.Message, StatusCodes.Status500InternalServerError, null, false);
        }
    }

    private static bool IsValidFormat(string pin)
    {
        return !string.IsNullOrEmpty(pin) && pin.Length == 6 && pin.All(char.IsDigit);
    }

    private static bool IsWeakPin(string pin)
    {
        if (pin.Distinct().Count() == 1)
            return true;

        return WeakSequentialRuns.Contains(pin);
    }

    private static HashSet<string> BuildSequentialRuns()
    {
        const string ascending = "0123456789";
        const string descending = "9876543210";
        var runs = new HashSet<string>();

        for (var i = 0; i <= ascending.Length - 6; i++)
        {
            runs.Add(ascending.Substring(i, 6));
            runs.Add(descending.Substring(i, 6));
        }

        return runs;
    }
}
