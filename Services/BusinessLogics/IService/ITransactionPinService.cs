using Trustesse.Ivoluntia.Commons.DTOs.Auth;
using Trustesse.Ivoluntia.Commons.Models.Response;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.IService;

public interface ITransactionPinService
{
    Task<GlobalRequestReponse<SetupTransactionPinResponse>> SetupTransactionPinAsync(SetupTransactionPinRequest request);
    Task<GlobalRequestReponse<VerifyTransactionPinResponse>> VerifyTransactionPinAsync(VerifyTransactionPinRequest request);
}
