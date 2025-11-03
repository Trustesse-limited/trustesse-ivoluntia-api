using System;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Trustesse.Ivoluntia.Commons.DTOs;

namespace Trustesse.Ivoluntia.API.Extensions;

public static class ResponseExtension
{
    public static IActionResult ToActionResult<T>(this ApiResponse<T> response)
    {
        return response.StatusCode switch
        {
            StatusCodes.Status400BadRequest => new BadRequestObjectResult(response),
            StatusCodes.Status401Unauthorized => new UnauthorizedObjectResult(response),
            StatusCodes.Status404NotFound => new NotFoundObjectResult(response),
            StatusCodes.Status409Conflict => new ConflictObjectResult(response),
            _ => new OkObjectResult(response)
        };
    }
}
