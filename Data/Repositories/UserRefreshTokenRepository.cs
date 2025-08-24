using System;
using Microsoft.EntityFrameworkCore;
using Trustesse.Ivoluntia.Data.DataContext;
using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Domain.IRepositories;

namespace Trustesse.Ivoluntia.Data.Repositories;

public class UserRefreshTokenRepository(iVoluntiaDataContext repositoryContext) : RepositoryBase<UserRefreshToken>(repositoryContext), IUserRefreshTokenRepository
{
    public void AddRefreshToken(UserRefreshToken userRefreshToken) => Create(userRefreshToken);

    public async Task<int> BulkUpdateAsync(string userId)
    {
        return await BulkUpdateAsync(
             rt => rt.UserId == userId,
             x => x.SetProperty(rt => rt.IsRevoked, true)
                 .SetProperty(rt => rt.DateUpdated, DateTime.UtcNow),
             CancellationToken.None);
    }

    public async Task<IEnumerable<UserRefreshToken>> GetActiveUserTokensAsync(string userId)
    {
        return await FindByCondition(x => x.UserId.Trim() == userId.Trim() && !x.IsRevoked, false).ToListAsync();
    }

    public async Task<UserRefreshToken?> GetUserRefreshTokenAsync(string refreshToken, string userId)
        => await FindByCondition(x => x.Token.Trim() == refreshToken.Trim() && x.UserId.Trim() == userId.Trim(), false)
                .Include(rt => rt.User)
                    .ThenInclude(u => u!.Foundation)
                .FirstOrDefaultAsync();

    public async Task<IEnumerable<UserRefreshToken>> GetUserRefreshTokensAsync(string userId)
    {
        return await FindByCondition(x => x.UserId.Trim() == userId.Trim(), false).ToListAsync();
    }

    public void UpdateRefreshToken(UserRefreshToken userRefreshToken)
    {
        Update(userRefreshToken);
    }
}
