namespace Crossessor.API.Features.Common.Models.Response;

public record EvaluationResponse(
    Guid Id,
    byte Accuracy,
    byte Completeness,
    byte Clarity,
    byte Neutrality,
    double OverallScore,
    AnswerResponse Answer
);