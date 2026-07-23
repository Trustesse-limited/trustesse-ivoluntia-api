using Microsoft.EntityFrameworkCore;
using Moq;
using Trustesse.Ivoluntia.Data.DataContext;
using Trustesse.Ivoluntia.Data.IRepositories;
using Trustesse.Ivoluntia.Data.Repositories;
using Trustesse.Ivoluntia.Domain.Entities;
using Xunit;

namespace Trustesse.Ivoluntia.Tests;

public class TransactionPinRepositoryTests
{
    private static iVoluntiaDataContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<iVoluntiaDataContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        var currentUserRepo = new Mock<ICurrentUserRepository>();
        currentUserRepo.Setup(x => x.GetUserFoundationId()).Returns("test-foundation");

        return new iVoluntiaDataContext(options, currentUserRepo.Object);
    }

    [Fact]
    public async Task AddAsync_Then_GetByExpressionAsync_ReturnsSameRow()
    {
        await using var context = CreateContext(nameof(AddAsync_Then_GetByExpressionAsync_ReturnsSameRow));
        var repo = new TransactionPinRepository(context);

        var pin = new TransactionPin
        {
            UserId = "user-1",
            PinHash = "hashed-value",
            CreatedDate = DateTime.UtcNow
        };

        await repo.AddAsync(pin);
        await context.SaveChangesAsync();

        var result = await repo.GetByExpressionAsync(x => x.UserId == "user-1");

        Assert.NotNull(result);
        Assert.Equal("hashed-value", result!.PinHash);
    }
}
