using Microsoft.EntityFrameworkCore;
using Trustesse.Ivoluntia.Data.DataContext;
using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Domain.IRepositories;

namespace Trustesse.Ivoluntia.Data.Repositories;

public class RefreshTokenRepository : GenericRepository<UserRefreshToken>, IRefreshTokenRepository
{
    private readonly iVoluntiaDataContext _context;
    public RefreshTokenRepository(iVoluntiaDataContext context) : base(context)
    {
        _context = context;
    }

    public async Task<UserRefreshToken> GetActiveUserTokensAsync(string userId)
    {
        return await GetByExpressionAsync(x => x.UserId.Trim() == userId.Trim() && !x.IsRevoked);

    }

    public async Task<UserRefreshToken?> GetUserRefreshTokenAsync(string refreshToken, string userId)
    {
        var query = GetByExpression(x => x.Token.Trim() == refreshToken.Trim() && x.UserId.Trim() == userId.Trim());

        return await query
            .Include(rt => rt.User)
                .ThenInclude(u => u!.Foundation)
            .FirstOrDefaultAsync();
    }

    public async Task<int> BulkUpdateAsync(string userId)
    {
        return await BulkUpdateAsync(
             rt => rt.UserId == userId,
             x => x.SetProperty(rt => rt.IsRevoked, true)
                 .SetProperty(rt => rt.DateUpdated, DateTime.UtcNow),
             CancellationToken.None);
    }

    public async Task<IEnumerable<UserRefreshToken>> GetUserRefreshTokensAsync(string userId)
    {
        return await GetByExpression(x => x.UserId.Trim() == userId.Trim()).ToListAsync();
    }
}