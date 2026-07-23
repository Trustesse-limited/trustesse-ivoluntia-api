using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Trustesse.Ivoluntia.API.Controllers.v1;
using Trustesse.Ivoluntia.Commons.DTOs.Auth;
using Trustesse.Ivoluntia.Commons.Extensions.Helpers;
using Trustesse.Ivoluntia.Services.BusinessLogics.IService;
using Xunit;

namespace Trustesse.Ivoluntia.Tests;

public class TransactionPinControllerTests
{
    [Fact]
    public async Task SetupTransactionPin_ServiceReturnsSuccess_ReturnsOk()
    {
        var service = new Mock<ITransactionPinService>();
        var request = new SetupTransactionPinRequest { Pin = "123890", ConfirmPin = "123890" };
        var response = ResponseHelper.BuildResponse("Transaction PIN created successfully.", StatusCodes.Status200OK,
            new SetupTransactionPinResponse { PinSetupComplete = true }, true);

        service.Setup(x => x.SetupTransactionPinAsync(request)).ReturnsAsync(response);

        var controller = new TransactionPinController(service.Object);

        var result = await controller.SetupTransactionPin(request);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task SetupTransactionPin_ServiceReturnsBadRequest_ReturnsBadRequest()
    {
        var service = new Mock<ITransactionPinService>();
        var request = new SetupTransactionPinRequest { Pin = "12", ConfirmPin = "12" };
        var response = ResponseHelper.BuildResponse<SetupTransactionPinResponse>("PIN must be exactly 6 digits.", StatusCodes.Status400BadRequest, null, false);

        service.Setup(x => x.SetupTransactionPinAsync(request)).ReturnsAsync(response);

        var controller = new TransactionPinController(service.Object);

        var result = await controller.SetupTransactionPin(request);

        Assert.IsType<BadRequestObjectResult>(result);
    }
}
