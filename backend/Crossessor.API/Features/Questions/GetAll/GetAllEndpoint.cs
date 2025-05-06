using Carter;
using Crossessor.API.Application.Utilities.Helpers;
using Mapster;
using MediatR;

namespace Crossessor.API.Features.Questions.GetAll;

public record GetAllRequest(
    int PageNumber = 1,
    int PageSize = 20);

public class GetAllEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet(pattern: "/Questions",
            handler: async ([AsParameters] GetAllRequest request, ISender sender) =>
            {
                var query = request.Adapt<GetAllQuery>();
                var result = await sender.Send(query);
                return result.FromResult();
            });
    }
}