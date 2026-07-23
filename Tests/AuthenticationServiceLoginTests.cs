using MapsterMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Linq.Expressions;
using Trustesse.Ivoluntia.Commons.DTOs.Auth;
using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Domain.IRepositories;
using Trustesse.Ivoluntia.Services.Abstractions;
using Trustesse.Ivoluntia.Services.BusinessLogics.Interfaces;
using Trustesse.Ivoluntia.Services.BusinessLogics.IService;
using Trustesse.Ivoluntia.Services.BusinessLogics.Service;
using Xunit;

namespace Trustesse.Ivoluntia.Tests;

public class AuthenticationServiceLoginTests
{
    private static Mock<UserManager<User>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<User>>();
        return new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);
    }

    private static (AuthenticationService sut, Mock<ITransactionPinRepository> transactionPinRepo) CreateSut(User user)
    {
        var uow = new Mock<IUnitOfWork>();
        var transactionPinRepo = new Mock<ITransactionPinRepository>();
        var userRepo = new Mock<IUserRepository>();

        uow.Setup(x => x.transactionPinRepo).Returns(transactionPinRepo.Object);
        uow.Setup(x => x.userRepo).Returns(userRepo.Object);
        uow.Setup(x => x.CompleteAsync()).ReturnsAsync(1);

        userRepo
            .Setup(x => x.GetUserByEmailWithFoundationAsync(user.Email!, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var userManager = CreateUserManagerMock();
        userManager.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Volunteer" });
        userManager.Setup(x => x.IsLockedOutAsync(user)).ReturnsAsync(false);
        userManager.Setup(x => x.CheckPasswordAsync(user, It.IsAny<string>())).ReturnsAsync(true);
        userManager.Setup(x => x.ResetAccessFailedCountAsync(user)).ReturnsAsync(IdentityResult.Success);

        var jwtTokenService = new Mock<IJwtTokenService>();
        jwtTokenService.Setup(x => x.GenerateAccessTokenAsync(It.IsAny<JwtClaimsModel>(), It.IsAny<string>())).Returns("access-token");
        jwtTokenService.Setup(x => x.GenerateRefreshTokenAsync(user.Id, It.IsAny<string>())).ReturnsAsync("refresh-token");

        var sut = new AuthenticationService(
            uow.Object,
            new Mock<IMapper>().Object,
            userManager.Object,
            jwtTokenService.Object,
            NullLogger<AuthenticationService>.Instance,
            new Mock<IOtpService>().Object,
            new Mock<INotificationService>().Object,
            new Mock<IEmailService>().Object,
            userRepo.Object,
            new Mock<IFileUploadService>().Object,
            new Mock<ICurrentUserService>().Object);

        return (sut, transactionPinRepo);
    }

    [Fact]
    public async Task LoginAsync_UserHasTransactionPin_HasSetUpPinIsTrue()
    {
        var user = new User { Id = "user-1", Email = "user@example.com", UserName = "user@example.com", IsActive = true };
        var (sut, transactionPinRepo) = CreateSut(user);

        transactionPinRepo
            .Setup(x => x.GetByExpressionAsync(It.IsAny<Expression<Func<TransactionPin, bool>>>()))
            .ReturnsAsync(new TransactionPin { UserId = "user-1", PinHash = "hash" });

        var result = await sut.LoginAsync(new LoginRequestModel { Email = user.Email!, Password = "P@ssw0rd!" }, CancellationToken.None);

        Assert.True(result.Data!.HasSetUpPin);
    }

    [Fact]
    public async Task LoginAsync_UserHasNoTransactionPin_HasSetUpPinIsFalse()
    {
        var user = new User { Id = "user-1", Email = "user@example.com", UserName = "user@example.com", IsActive = true };
        var (sut, transactionPinRepo) = CreateSut(user);

        transactionPinRepo
            .Setup(x => x.GetByExpressionAsync(It.IsAny<Expression<Func<TransactionPin, bool>>>()))
            .ReturnsAsync((TransactionPin?)null);

        var result = await sut.LoginAsync(new LoginRequestModel { Email = user.Email!, Password = "P@ssw0rd!" }, CancellationToken.None);

        Assert.False(result.Data!.HasSetUpPin);
    }
}
