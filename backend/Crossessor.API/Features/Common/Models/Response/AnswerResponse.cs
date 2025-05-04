using Crossessor.API.Domain.Enums;

namespace Crossessor.API.Features.Common.Models.Response;

public record AnswerResponse(
    Guid Id,
    string Text,
    ModelType ModelType,
    AnswerType AnswerType);