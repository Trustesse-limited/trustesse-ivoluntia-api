using Microsoft.AspNetCore.Mvc;

namespace Trustesse.Ivoluntia.API.Controllers.v1
{
    public class BaseController : ControllerBase
    {
        internal string GetLoggedInUserId()
        {
            return string.Empty;
        }
    }
}
