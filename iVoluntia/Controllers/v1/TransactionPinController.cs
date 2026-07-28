using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trustesse.Ivoluntia.API.Filters;
using Trustesse.Ivoluntia.Commons.DTOs.Auth;
using Trustesse.Ivoluntia.Services.BusinessLogics.IService;

namespace Trustesse.Ivoluntia.API.Controllers.v1
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionPinController : BaseController
    {
        private readonly ITransactionPinService _transactionPinService;

        public TransactionPinController(ITransactionPinService transactionPinService)
        {
            _transactionPinService = transactionPinService;
        }

        [HttpPost("setup-pin")]
        public async Task<IActionResult> SetupTransactionPin([FromBody] SetupTransactionPinRequest request)
            => BuildHttpResponse(await _transactionPinService.SetupTransactionPinAsync(request));

        [HttpPost("pin-verification")]
        [RateLimit(MaxRequests = 10, WindowSeconds = 60)]
        public async Task<IActionResult> VerifyTransactionPin([FromBody] VerifyTransactionPinRequest request)
            => BuildHttpResponse(await _transactionPinService.VerifyTransactionPinAsync(request));
    }
}
