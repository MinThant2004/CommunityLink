using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CommunityLink.Shared;

[ApiController]
public abstract class BaseController : ControllerBase
{
    protected IActionResult ToActionResult<T>(Result<T> result) => result.Status switch
    {
        ResultStatus.Success => Ok(result),
        ResultStatus.ValidationError => BadRequest(result),
        ResultStatus.Unauthorized => Unauthorized(result),
        ResultStatus.Forbidden => StatusCode(StatusCodes.Status403Forbidden, result),
        ResultStatus.NotFound => NotFound(result),
        ResultStatus.Conflict => Conflict(result),
        ResultStatus.RateLimited => StatusCode(StatusCodes.Status429TooManyRequests, result),
        _ => StatusCode(StatusCodes.Status500InternalServerError, result)
    };

    protected IActionResult ToActionResult(Result result) => result.Status switch
    {
        ResultStatus.Success => Ok(result),
        ResultStatus.ValidationError => BadRequest(result),
        ResultStatus.Unauthorized => Unauthorized(result),
        ResultStatus.Forbidden => StatusCode(StatusCodes.Status403Forbidden, result),
        ResultStatus.NotFound => NotFound(result),
        ResultStatus.Conflict => Conflict(result),
        ResultStatus.RateLimited => StatusCode(StatusCodes.Status429TooManyRequests, result),
        _ => StatusCode(StatusCodes.Status500InternalServerError, result)
    };
}