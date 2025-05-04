namespace Crossessor.API.Features.Common.Models.Response;

public record EvaluationResponse(
    byte Accuracy,
    byte Completeness,
    byte Clarity,
    byte Neutrality,
    double OverallScore,
    AnswerResponse Answer
);