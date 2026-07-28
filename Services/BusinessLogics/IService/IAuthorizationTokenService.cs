using Trustesse.Ivoluntia.Commons.DTOs.Auth;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.IService;

public interface IAuthorizationTokenService
{
    Task<string> GenerateTokenAsync(string userId, string purpose);
    Task<AuthorizationTokenResult> VerifyTokenAsync(string token, string userId, string purpose);
}
