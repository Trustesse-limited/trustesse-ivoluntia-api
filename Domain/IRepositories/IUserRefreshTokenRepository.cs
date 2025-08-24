using System;
using Trustesse.Ivoluntia.Domain.Entities;

namespace Trustesse.Ivoluntia.Domain.IRepositories;

public interface IUserRefreshTokenRepository
{
    void AddRefreshToken(UserRefreshToken userRefreshToken);
    void UpdateRefreshToken(UserRefreshToken userRefreshToken);
    Task<UserRefreshToken?> GetUserRefreshTokenAsync(string refreshToken, string userId);
    Task<IEnumerable<UserRefreshToken>> GetUserRefreshTokensAsync(string userId);

    Task<IEnumerable<UserRefreshToken>> GetActiveUserTokensAsync(string userId);

    Task<int> BulkUpdateAsync(string userId);
}
