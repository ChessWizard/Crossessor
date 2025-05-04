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
) : ICommand<Result<CrossCompareCommandResult>>;

public record CrossCompareCommandResult(
    List<EvaluationResponse> Evaluations
);

public record EvaluationResult(
    byte Accuracy,
    byte Completeness,
    byte Clarity,
    byte Neutrality,
    double OverallScore,
    string Comment
);

public class CrossCompareHandler(Kernel kernel,
    CrossessorDbContext dbContext) : ICommandHandler<CrossCompareCommand, Result<CrossCompareCommandResult>>
{
    public async Task<Result<CrossCompareCommandResult>> Handle(
        CrossCompareCommand request,
        CancellationToken cancellationToken)
    {
        var answers = await GetAnswersAsync(request.AnswerIds, cancellationToken);
        if (answers.Count == 0)
        {
            return Result<CrossCompareCommandResult>.Error(
                "No answers found", 
                (int)HttpStatusCode.BadRequest);
        }

        var evaluations = await ProcessEvaluationsAsync(
            request.SystemBehaviourType, 
            answers, 
            cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<CrossCompareCommandResult>.Success(
            new CrossCompareCommandResult(evaluations), 
            (int)HttpStatusCode.OK);
    }

    private async Task<List<AnswerEntity>> GetAnswersAsync(
        List<Guid> answerIds,
        CancellationToken cancellationToken)
    {
        return await dbContext.Answers
            .Include(a => a.Question)
            .Where(answer => answerIds.Contains(answer.Id))
            .ToListAsync(cancellationToken);
    }

    private async Task<List<EvaluationResponse>> ProcessEvaluationsAsync(
        SystemBehaviourType systemBehaviourType,
        List<AnswerEntity> answers,
        CancellationToken cancellationToken)
    {
        var evaluations = new List<EvaluationResponse>();

        foreach (var answer in answers)
        {
            var crossAnswer = GetCrossAnswer(answers, answer);
            var evaluation = await CreateEvaluationAsync(
                systemBehaviourType,
                answer,
                crossAnswer,
                cancellationToken);

            evaluations.Add(evaluation);
        }

        return evaluations;
    }

    private static AnswerEntity GetCrossAnswer(List<AnswerEntity> answers, AnswerEntity currentAnswer)
    {
        return answers.FirstOrDefault(a => a.ModelType != currentAnswer.ModelType)
            ?? throw new InvalidOperationException("No cross answer found");
    }

    private async Task<EvaluationResponse> CreateEvaluationAsync(
        SystemBehaviourType systemBehaviourType,
        AnswerEntity answer,
        AnswerEntity crossAnswer,
        CancellationToken cancellationToken)
    {
        var evaluationResponse = await GetEvaluationResponseAsync(
            systemBehaviourType,
            answer.Text,
            crossAnswer.ModelType,
            cancellationToken);

        var evaluationResult = ParseEvaluationResponse(evaluationResponse);
        var evaluation = await CreateAndSaveEvaluationAsync(
            answer,
            crossAnswer,
            evaluationResult,
            cancellationToken);

        return evaluation.Adapt<EvaluationResponse>();
    }

    private async Task<string> GetEvaluationResponseAsync(
        SystemBehaviourType systemBehaviourType,
        string answerText,
        ModelType evaluatorModelType,
        CancellationToken cancellationToken)
    {
        var evaluatorServiceId = evaluatorModelType.GetModelByType();
        var chatProvider = kernel.GetRequiredService<IChatCompletionService>(evaluatorServiceId);

        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage(systemBehaviourType.GetSystemPromptByBehaviour());
        chatHistory.AddUserMessage(UserBehaviourType.Evaluation.GetUserPromptByBehaviour(answerText));

        var response = await chatProvider.GetChatMessageContentAsync(
            chatHistory, 
            cancellationToken: cancellationToken);

        return response?.Content ?? "{}";
    }

    private static EvaluationResult ParseEvaluationResponse(string response)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(response))
            {
                throw new InvalidOperationException("Empty response received");
            }

            var cleanResponse = response.ExtractPromptResponseJson();

            using var json = JsonDocument.Parse(cleanResponse);
            var root = json.RootElement;

            if (!root.TryGetProperty("accuracy", out var accuracyElement) ||
                !root.TryGetProperty("completeness", out var completenessElement) ||
                !root.TryGetProperty("clarity", out var clarityElement) ||
                !root.TryGetProperty("neutrality", out var neutralityElement) ||
                !root.TryGetProperty("comment", out var commentElement))
            {
                throw new InvalidOperationException("Required properties are missing in the response");
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
                throw new InvalidOperationException("Scores must be between 0 and 100");
            }
            
            var overallScore = EvaluationHelper.CalculateOverallScore(accuracy, completeness, clarity, neutrality);
            
            return new EvaluationResult(
                (byte)accuracy,
                (byte)completeness,
                (byte)clarity,
                (byte)neutrality,
                overallScore,
                comment
            );
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Invalid JSON format: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error parsing evaluation response: {ex.Message}", ex);
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