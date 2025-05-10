using System.Text.Json.Serialization;

namespace Crossessor.API.Application.Models.Result;

public class BaseResult<TData>
{
    public TData Data { get; set; }

    public string Message { get; set; }

    public int HttpStatusCode { get; set; }

    [JsonIgnore] public bool IsSuccessful { get; set; } = true;

    public ErrorResult ErrorDto { get; set; }
}