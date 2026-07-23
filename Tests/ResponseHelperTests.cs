using Microsoft.AspNetCore.Http;
using Trustesse.Ivoluntia.Commons.Extensions.Helpers;
using Xunit;

namespace Trustesse.Ivoluntia.Tests;

public class ResponseHelperTests
{
    [Fact]
    public void BuildResponse_SetsAllFieldsFromArguments()
    {
        var result = ResponseHelper.BuildResponse("ok", StatusCodes.Status200OK, "payload", true);

        Assert.True(result.isSuccessfull);
        Assert.Equal("ok", result.Message);
        Assert.Equal(StatusCodes.Status200OK, result.ResponseCode);
        Assert.Equal("payload", result.Data);
        Assert.Empty(result.Errors);
    }
}
