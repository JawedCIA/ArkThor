using Microsoft.AspNetCore.Mvc;

namespace ArkThor.API.Controllers
{
    public class BaseApiController : Controller
    {
        protected IActionResult HandleException(
             Exception ex, string msg)
        {
            IActionResult ret;

            // TODO: Publish exceptions here

            // Create new exception with generic message              
            ret = StatusCode(StatusCodes.Status500InternalServerError,
                new Exception(msg));
            return ret;
        }
    }
}
