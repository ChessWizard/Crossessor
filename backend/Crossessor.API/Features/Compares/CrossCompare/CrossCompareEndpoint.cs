using Carter;
using Crossessor.API.Application.Enums;
using Crossessor.API.Application.Utilities.Helpers;
using Mapster;
using MediatR;

namespace Crossessor.API.Features.Compares.CrossCompare;

public record CrossCompareRequest(
    SystemBehaviourType SystemBehaviourType,
    List<Guid> AnswerIds
);

public class CrossCompareEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost(pattern: "/Compare",
            handler: async (CrossCompareRequest request, ISender sender) =>
            {
                var command = request.Adapt<CrossCompareCommand>();
                var result = await sender.Send(command);
                return result.FromResult();
            });
    }
}