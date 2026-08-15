using Trustesse.Ivoluntia.Data.IRepositories;
using Trustesse.Ivoluntia.Domain.Entities;


public interface IRefreshTokenRepository : IGenericRepository<UserRefreshToken>
{
    Task<int> BulkUpdateAsync(string userId);
    Task<UserRefreshToken> GetActiveUserTokensAsync(string userId);
    Task<UserRefreshToken> GetUserRefreshTokenAsync(string refreshToken, string userId);
}