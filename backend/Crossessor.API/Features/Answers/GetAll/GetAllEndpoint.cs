using Carter;
using Crossessor.API.Application.Models.Result;
using Crossessor.API.Application.Utilities.Helpers;
using Crossessor.API.Domain.Enums;
using Crossessor.API.Features.Answers.GetAll;
using Mapster;
using MediatR;

namespace Crossessor.API.Features.Answers.GetAll;

public record GetAllRequest(
    Guid? QuestionId,
    AnswerType? AnswerType,
    ModelType? ModelType,
    int PageNumber = 1,
    int PageSize = 20);

public class GetAllEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet(pattern: "/Answers",
            handler: async ([AsParameters] GetAllRequest request, ISender sender) =>
            {
                var query = request.Adapt<GetAllQuery>();
                var result = await sender.Send(query);
                return result.FromResult();
            });
    }
}