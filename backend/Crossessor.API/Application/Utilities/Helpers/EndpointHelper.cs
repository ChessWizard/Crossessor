using Crossessor.API.Application.Models.Result;

namespace Crossessor.API.Application.Utilities.Helpers;

public static class EndpointHelper
{
    public static IResult FromResult<T>(this BaseResult<T> result)
        => Results.Json(result, statusCode: result.HttpStatusCode);
}