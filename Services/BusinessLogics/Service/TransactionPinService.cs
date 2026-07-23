using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Trustesse.Ivoluntia.Commons.DTOs.Auth;
using Trustesse.Ivoluntia.Commons.Extensions.Helpers;
using Trustesse.Ivoluntia.Commons.Models.Response;
using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Domain.IRepositories;
using Trustesse.Ivoluntia.Services.BusinessLogics.Interfaces;
using Trustesse.Ivoluntia.Services.BusinessLogics.IService;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.Service;

public class TransactionPinService : ITransactionPinService
{
    private static readonly HashSet<string> WeakSequentialRuns = BuildSequentialRuns();

    private readonly ICurrentUserService _currentUserService;
    private readonly IPasswordHasher<string> _passwordHasher;
    private readonly IUnitOfWork _uow;

    public TransactionPinService(
        ICurrentUserService currentUserService,
        IPasswordHasher<string> passwordHasher,
        IUnitOfWork uow)
    {
        _currentUserService = currentUserService;
        _passwordHasher = passwordHasher;
        _uow = uow;
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
