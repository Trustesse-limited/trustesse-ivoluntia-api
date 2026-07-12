using Trustesse.Ivoluntia.Domain.Entities;

namespace Trustesse.Ivoluntia.Domain.IRepositories;

public interface IRefreshTokenRepository : IGenericRepository<UserRefreshToken>
{
    Task<int> BulkUpdateAsync(string userId);
    Task<UserRefreshToken> GetActiveUserTokensAsync(string userId);
    Task<UserRefreshToken> GetUserRefreshTokenAsync(string refreshToken, string userId);
}