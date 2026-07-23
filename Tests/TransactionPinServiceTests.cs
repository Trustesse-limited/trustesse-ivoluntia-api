using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Moq;
using System.Linq.Expressions;
using Trustesse.Ivoluntia.Commons.DTOs.Auth;
using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Domain.IRepositories;
using Trustesse.Ivoluntia.Services.BusinessLogics.Interfaces;
using Trustesse.Ivoluntia.Services.BusinessLogics.Service;
using Xunit;

namespace Trustesse.Ivoluntia.Tests;

public class TransactionPinServiceTests
{
    private readonly Mock<ICurrentUserService> _currentUserService = new();
    private readonly Mock<IPasswordHasher<string>> _passwordHasher = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IUserSecurityQuestionRepository> _userSecurityQuestionRepo = new();
    private readonly Mock<ITransactionPinRepository> _transactionPinRepo = new();

    private TransactionPinService CreateSut()
    {
        _uow.Setup(x => x.userSecurityQuestionRepo).Returns(_userSecurityQuestionRepo.Object);
        _uow.Setup(x => x.transactionPinRepo).Returns(_transactionPinRepo.Object);
        _uow.Setup(x => x.CompleteAsync()).ReturnsAsync(1);

        _passwordHasher
            .Setup(x => x.HashPassword(It.IsAny<string>(), It.IsAny<string>()))
            .Returns<string, string>((_, pin) => $"HASHED:{pin}");

        return new TransactionPinService(_currentUserService.Object, _passwordHasher.Object, _uow.Object);
    }

    private void SetUpUser(string? userId, bool hasSecurityQuestions, TransactionPin? existingPin)
    {
        _currentUserService.Setup(x => x.GetUserId()).Returns(userId!);

        _userSecurityQuestionRepo
            .Setup(x => x.GetListByExpressionAsync(
                It.IsAny<Expression<Func<UserSecurityQuestion, bool>>>(),
                It.IsAny<Expression<Func<UserSecurityQuestion, object>>[]>()))
            .ReturnsAsync(hasSecurityQuestions
                ? new List<UserSecurityQuestion> { new() { UserId = userId!, SecurityQuestionId = "q1", AnswerHash = "h" } }
                : new List<UserSecurityQuestion>());

        _transactionPinRepo
            .Setup(x => x.GetByExpressionAsync(It.IsAny<Expression<Func<TransactionPin, bool>>>()))
            .ReturnsAsync(existingPin);
    }

    [Fact]
    public async Task SetupTransactionPinAsync_NoAuthenticatedUser_ReturnsBadRequest()
    {
        var sut = CreateSut();
        SetUpUser(userId: null, hasSecurityQuestions: true, existingPin: null);

        var result = await sut.SetupTransactionPinAsync(new SetupTransactionPinRequest { Pin = "123890", ConfirmPin = "123890" });

        Assert.False(result.isSuccessfull);
        Assert.Equal(StatusCodes.Status400BadRequest, result.ResponseCode);
    }

    [Fact]
    public async Task SetupTransactionPinAsync_SecurityQuestionsNotConfigured_ReturnsBadRequest()
    {
        var sut = CreateSut();
        SetUpUser(userId: "user-1", hasSecurityQuestions: false, existingPin: null);

        var result = await sut.SetupTransactionPinAsync(new SetupTransactionPinRequest { Pin = "123890", ConfirmPin = "123890" });

        Assert.False(result.isSuccessfull);
        Assert.Equal(StatusCodes.Status400BadRequest, result.ResponseCode);
        Assert.Contains("security questions", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SetupTransactionPinAsync_PinAlreadyExists_ReturnsBadRequest()
    {
        var sut = CreateSut();
        SetUpUser(userId: "user-1", hasSecurityQuestions: true, existingPin: new TransactionPin { UserId = "user-1", PinHash = "existing" });

        var result = await sut.SetupTransactionPinAsync(new SetupTransactionPinRequest { Pin = "123890", ConfirmPin = "123890" });

        Assert.False(result.isSuccessfull);
        Assert.Equal(StatusCodes.Status400BadRequest, result.ResponseCode);
        Assert.Contains("already", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("12345")]
    [InlineData("1234567")]
    [InlineData("12a456")]
    [InlineData("")]
    public async Task SetupTransactionPinAsync_InvalidFormat_ReturnsBadRequest(string pin)
    {
        var sut = CreateSut();
        SetUpUser(userId: "user-1", hasSecurityQuestions: true, existingPin: null);

        var result = await sut.SetupTransactionPinAsync(new SetupTransactionPinRequest { Pin = pin, ConfirmPin = pin });

        Assert.False(result.isSuccessfull);
        Assert.Equal(StatusCodes.Status400BadRequest, result.ResponseCode);
    }

    [Fact]
    public async Task SetupTransactionPinAsync_ConfirmPinMismatch_ReturnsBadRequest()
    {
        var sut = CreateSut();
        SetUpUser(userId: "user-1", hasSecurityQuestions: true, existingPin: null);

        var result = await sut.SetupTransactionPinAsync(new SetupTransactionPinRequest { Pin = "123890", ConfirmPin = "123891" });

        Assert.False(result.isSuccessfull);
        Assert.Equal(StatusCodes.Status400BadRequest, result.ResponseCode);
        Assert.Contains("match", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("000000")]
    [InlineData("999999")]
    [InlineData("123456")]
    [InlineData("456789")]
    [InlineData("987654")]
    [InlineData("543210")]
    public async Task SetupTransactionPinAsync_WeakPin_ReturnsBadRequest(string pin)
    {
        var sut = CreateSut();
        SetUpUser(userId: "user-1", hasSecurityQuestions: true, existingPin: null);

        var result = await sut.SetupTransactionPinAsync(new SetupTransactionPinRequest { Pin = pin, ConfirmPin = pin });

        Assert.False(result.isSuccessfull);
        Assert.Equal(StatusCodes.Status400BadRequest, result.ResponseCode);
    }

    [Fact]
    public async Task SetupTransactionPinAsync_HappyPath_HashesPinAndPersists()
    {
        var sut = CreateSut();
        SetUpUser(userId: "user-1", hasSecurityQuestions: true, existingPin: null);

        TransactionPin? captured = null;
        _transactionPinRepo
            .Setup(x => x.AddAsync(It.IsAny<TransactionPin>()))
            .Callback<TransactionPin>(p => captured = p)
            .Returns(Task.CompletedTask);

        var result = await sut.SetupTransactionPinAsync(new SetupTransactionPinRequest { Pin = "123890", ConfirmPin = "123890" });

        Assert.True(result.isSuccessfull);
        Assert.Equal(StatusCodes.Status200OK, result.ResponseCode);
        Assert.True(result.Data!.PinSetupComplete);

        Assert.NotNull(captured);
        Assert.Equal("user-1", captured!.UserId);
        Assert.NotEqual("123890", captured.PinHash);
        Assert.Equal("HASHED:123890", captured.PinHash);

        _transactionPinRepo.Verify(x => x.AddAsync(It.IsAny<TransactionPin>()), Times.Once);
        _uow.Verify(x => x.CompleteAsync(), Times.Once);
    }
}
