using System.Net;
using System.Text.Json;
using Crossessor.API.Application.Enums;
using Crossessor.API.Application.Interfaces.CQRS.Command;
using Crossessor.API.Application.Models.Result;
using Crossessor.API.Application.Utilities.Helpers;
using Crossessor.API.Domain.Entities;
using Crossessor.API.Domain.Enums;
using Crossessor.API.Features.Common.Models.Response;
using Crossessor.API.Infrastructure.Data.Context;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Crossessor.API.Features.Compares.CrossCompare;

public record CrossCompareCommand(
    SystemBehaviourType SystemBehaviourType,
    List<Guid> AnswerIds
) : ICommand<Result<List<EvaluationResponse>>>;

public record EvaluationResult(
    byte Accuracy,
    byte Completeness,
    byte Clarity,
    byte Neutrality,
    double OverallScore,
    string Comment
);

public class CrossCompareHandler(Kernel kernel,
    CrossessorDbContext dbContext) : ICommandHandler<CrossCompareCommand, Result<List<EvaluationResponse>>>
{
    public async Task<Result<List<EvaluationResponse>>> Handle(
        CrossCompareCommand request,
        CancellationToken cancellationToken)
    {
        var answersResult = await GetAnswersAsync(request.AnswerIds, cancellationToken);
        if (!answersResult.IsSuccessful)
        {
            return Result<List<EvaluationResponse>>.Error(
                answersResult.Message,
                answersResult.HttpStatusCode);
        }

        var evaluationsResult = await ProcessEvaluationsAsync(
            request.SystemBehaviourType,
            answersResult.Data,
            cancellationToken);

        if (!evaluationsResult.IsSuccessful)
        {
            return Result<List<EvaluationResponse>>.Error(
                evaluationsResult.Message,
                evaluationsResult.HttpStatusCode);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<List<EvaluationResponse>>.Success(
            evaluationsResult.Data,
            (int)HttpStatusCode.OK);
    }

    private async Task<Result<List<AnswerEntity>>> GetAnswersAsync(
        List<Guid> answerIds,
        CancellationToken cancellationToken)
    {
        var answers = await dbContext.Answers
            .Include(a => a.Question)
            .Where(answer => answerIds.Contains(answer.Id))
            .ToListAsync(cancellationToken);

        if (answers.Count == 0)
        {
            return Result<List<AnswerEntity>>.Error(
                "No answers found",
                (int)HttpStatusCode.BadRequest);
        }

        return Result<List<AnswerEntity>>.Success(answers);
    }

    private async Task<Result<List<EvaluationResponse>>> ProcessEvaluationsAsync(
        SystemBehaviourType systemBehaviourType,
        List<AnswerEntity> answers,
        CancellationToken cancellationToken)
    {
        var evaluations = new List<EvaluationResponse>();

        foreach (var answer in answers)
        {
            var crossAnswerResult = GetCrossAnswer(answers, answer);
            if (!crossAnswerResult.IsSuccessful)
            {
                return Result<List<EvaluationResponse>>.Error(
                    crossAnswerResult.Message,
                    crossAnswerResult.HttpStatusCode);
            }

            var evaluationResult = await CreateEvaluationAsync(
                systemBehaviourType,
                answer,
                crossAnswerResult.Data,
                cancellationToken);

            if (!evaluationResult.IsSuccessful)
            {
                return Result<List<EvaluationResponse>>.Error(
                    evaluationResult.Message,
                    evaluationResult.HttpStatusCode);
            }

            evaluations.Add(evaluationResult.Data);
        }

        return Result<List<EvaluationResponse>>.Success(evaluations);
    }

    private static Result<AnswerEntity> GetCrossAnswer(List<AnswerEntity> answers, AnswerEntity currentAnswer)
    {
        var crossAnswer = answers.FirstOrDefault(a => a.ModelType != currentAnswer.ModelType);
        if (crossAnswer == null)
        {
            return Result<AnswerEntity>.Error(
                "No cross answer found",
                (int)HttpStatusCode.BadRequest);
        }

        return Result<AnswerEntity>.Success(crossAnswer);
    }

    private async Task<Result<EvaluationResponse>> CreateEvaluationAsync(
        SystemBehaviourType systemBehaviourType,
        AnswerEntity answer,
        AnswerEntity crossAnswer,
        CancellationToken cancellationToken)
    {
        var evaluationResponseResult = await GetEvaluationResponseAsync(
            systemBehaviourType,
            answer.Text,
            crossAnswer.ModelType,
            cancellationToken);

        if (!evaluationResponseResult.IsSuccessful)
        {
            return Result<EvaluationResponse>.Error(
                evaluationResponseResult.Message,
                evaluationResponseResult.HttpStatusCode);
        }

        var evaluationResult = ParseEvaluationResponse(evaluationResponseResult.Data);
        if (!evaluationResult.IsSuccessful)
        {
            return Result<EvaluationResponse>.Error(
                evaluationResult.Message,
                evaluationResult.HttpStatusCode);
        }

        var evaluation = await CreateAndSaveEvaluationAsync(
            answer,
            crossAnswer,
            evaluationResult.Data,
            cancellationToken);

        return Result<EvaluationResponse>.Success(evaluation.Adapt<EvaluationResponse>());
    }

    private async Task<Result<string>> GetEvaluationResponseAsync(
        SystemBehaviourType systemBehaviourType,
        string answerText,
        ModelType evaluatorModelType,
        CancellationToken cancellationToken)
    {
        try
        {
            var evaluatorServiceId = evaluatorModelType.GetModelByType();
            var chatProvider = kernel.GetRequiredService<IChatCompletionService>(evaluatorServiceId);

            var chatHistory = new ChatHistory();
            chatHistory.AddSystemMessage(systemBehaviourType.GetSystemPromptByBehaviour());
            chatHistory.AddUserMessage(UserBehaviourType.Evaluation.GetUserPromptByBehaviour(answerText));

            var response = await chatProvider.GetChatMessageContentAsync(
                chatHistory,
                cancellationToken: cancellationToken);

            if (string.IsNullOrWhiteSpace(response?.Content))
            {
                return Result<string>.Error(
                    "Empty response received from chat provider",
                    (int)HttpStatusCode.BadRequest);
            }

            return Result<string>.Success(response.Content);
        }
        catch (Exception ex)
        {
            return Result<string>.Error(
                $"Error getting evaluation response: {ex.Message}",
                (int)HttpStatusCode.InternalServerError);
        }
    }

    private static Result<EvaluationResult> ParseEvaluationResponse(string response)
    {
        if (string.IsNullOrWhiteSpace(response))
        {
            return Result<EvaluationResult>.Error(
                "Empty response received",
                (int)HttpStatusCode.BadRequest);
        }

        try
        {
            var cleanResponse = response.ExtractPromptResponseJson();

            using var json = JsonDocument.Parse(cleanResponse);
            var root = json.RootElement;

            if (!root.TryGetProperty("accuracy", out var accuracyElement) ||
                !root.TryGetProperty("completeness", out var completenessElement) ||
                !root.TryGetProperty("clarity", out var clarityElement) ||
                !root.TryGetProperty("neutrality", out var neutralityElement) ||
                !root.TryGetProperty("comment", out var commentElement))
            {
                return Result<EvaluationResult>.Error(
                    "Required properties are missing in the response",
                    (int)HttpStatusCode.BadRequest);
            }

            var accuracy = accuracyElement.GetInt32();
            var completeness = completenessElement.GetInt32();
            var clarity = clarityElement.GetInt32();
            var neutrality = neutralityElement.GetInt32();
            var comment = commentElement.GetString() ?? string.Empty;

            if (accuracy < 0 || accuracy > 100 ||
                completeness < 0 || completeness > 100 ||
                clarity < 0 || clarity > 100 ||
                neutrality < 0 || neutrality > 100)
            {
                return Result<EvaluationResult>.Error(
                    "Scores must be between 0 and 100",
                    (int)HttpStatusCode.BadRequest);
            }

            var overallScore = EvaluationHelper.CalculateOverallScore(accuracy, completeness, clarity, neutrality);

            return Result<EvaluationResult>.Success(new EvaluationResult(
                (byte)accuracy,
                (byte)completeness,
                (byte)clarity,
                (byte)neutrality,
                overallScore,
                comment
            ));
        }
        catch (JsonException ex)
        {
            return Result<EvaluationResult>.Error(
                $"Invalid JSON format: {ex.Message}",
                (int)HttpStatusCode.BadRequest);
        }
        catch (Exception ex)
        {
            return Result<EvaluationResult>.Error(
                $"Error parsing evaluation response: {ex.Message}",
                (int)HttpStatusCode.InternalServerError);
        }
    }

    private async Task<EvaluationEntity> CreateAndSaveEvaluationAsync(
        AnswerEntity answer,
        AnswerEntity crossAnswer,
        EvaluationResult evaluationResult,
        CancellationToken cancellationToken)
    {
        var evaluationAnswer = new AnswerEntity
        {
            Text = evaluationResult.Comment,
            ModelType = crossAnswer.ModelType,
            AnswerType = AnswerType.Evaluation,
            Question = answer.Question
        };

        await dbContext.Answers.AddAsync(evaluationAnswer, cancellationToken);

        var evaluation = new EvaluationEntity
        {
            Accuracy = evaluationResult.Accuracy,
            Completeness = evaluationResult.Completeness,
            Clarity = evaluationResult.Clarity,
            Neutrality = evaluationResult.Neutrality,
            OverallScore = evaluationResult.OverallScore,
            Answer = evaluationAnswer,
            TargetAnswer = answer
        };

        await dbContext.Evaluations.AddAsync(evaluation, cancellationToken);

        return evaluation;
    }
}