using Microsoft.AspNetCore.Mvc;
using Trustesse.Ivoluntia.Commons.Models.Response;

namespace Trustesse.Ivoluntia.API.Controllers.v1
{
    public class BaseController : ControllerBase
    {
        internal string GetLoggedInUserId()
        {
            return string.Empty;
        }
        public IActionResult BuildHttpResponse<T>(GlobalRequestReponse<T> requestReponse)
        {
            return requestReponse.ResponseCode switch
            {
                StatusCodes.Status200OK => Ok(requestReponse),
                StatusCodes.Status201Created => Created("", requestReponse),
                StatusCodes.Status404NotFound => NotFound(requestReponse),
                StatusCodes.Status401Unauthorized => Unauthorized(requestReponse),
                StatusCodes.Status400BadRequest => BadRequest(requestReponse),
                StatusCodes.Status413PayloadTooLarge => BadRequest(requestReponse),
                StatusCodes.Status409Conflict => Conflict(requestReponse),
                StatusCodes.Status415UnsupportedMediaType => BadRequest(requestReponse),
                StatusCodes.Status204NoContent => NoContent(),
            };
        }
    }
}
