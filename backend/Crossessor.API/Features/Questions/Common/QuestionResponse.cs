namespace Crossessor.API.Features.Questions.Common;

public record QuestionResponse(Guid Id,
    string Text,
    DateTimeOffset CreatedDate,
    DateTimeOffset? DeletedDate);