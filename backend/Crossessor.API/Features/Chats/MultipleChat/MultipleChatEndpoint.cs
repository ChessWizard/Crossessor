using Carter;
using Crossessor.API.Application.Enums;
using Crossessor.API.Application.Utilities.Helpers;
using Crossessor.API.Domain.Enums;
using Mapster;
using MediatR;

namespace Crossessor.API.Features.Chats.MultipleChat;

public record MultipleChatRequest(
    string Question,
    SystemBehaviourType SystemBehaviourType,
    List<ModelType> TargetModelTypes);

public class MultipleChatEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost(pattern: "/Chat",
            handler: async (MultipleChatRequest request, ISender sender) =>
            {
                var command = request.Adapt<MultipleChatCommand>();
                var result = await sender.Send(command);
                return result.FromResult();
            });
    }
}