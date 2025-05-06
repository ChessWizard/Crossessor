using System.Net;
using Crossessor.API.Application.Enums;
using Crossessor.API.Application.Interfaces.CQRS.Command;
using Crossessor.API.Application.Models.Result;
using Crossessor.API.Application.Utilities.Helpers;
using Crossessor.API.Domain.Entities;
using Crossessor.API.Domain.Enums;
using Crossessor.API.Features.Common.Models.Response;
using Crossessor.API.Infrastructure.Data.Context;
using Mapster;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Crossessor.API.Features.Chats.MultipleChat;

public record MultipleChatCommand(
    string Question,
    SystemBehaviourType SystemBehaviourType,
    List<ModelType> TargetModelTypes) : ICommand<Result<List<AnswerResponse>>>;

public class MultipleChatHandler(Kernel kernel,
    CrossessorDbContext dbContext) : ICommandHandler<MultipleChatCommand, Result<List<AnswerResponse>>>
{
    public async Task<Result<List<AnswerResponse>>> Handle(
        MultipleChatCommand request, 
        CancellationToken cancellationToken)
    {
        var question = await CreateQuestionAsync(request.Question, cancellationToken);
        var answers = await ProcessChatResponsesAsync(request, question, cancellationToken);
        
        await dbContext.SaveChangesAsync(cancellationToken);
        
        return Result<List<AnswerResponse>>.Success(
            answers, 
            (int)HttpStatusCode.OK);
    }

    private async Task<QuestionEntity> CreateQuestionAsync(
        string questionText, 
        CancellationToken cancellationToken)
    {
        var question = new QuestionEntity { Text = questionText };
        await dbContext.Questions
                       .AddAsync(question, cancellationToken);
        return question;
    }

    private async Task<List<AnswerResponse>> ProcessChatResponsesAsync(
        MultipleChatCommand request,
        QuestionEntity question,
        CancellationToken cancellationToken)
    {
        var answers = new List<AnswerResponse>();

        foreach (var modelType in request.TargetModelTypes)
        {
            var chatResponse = await GetChatResponseAsync(request, modelType, cancellationToken);
            var answer = await CreateAnswerAsync(question, modelType, chatResponse, cancellationToken);
            
            answers.Add(answer.Adapt<AnswerResponse>());
        }

        return answers;
    }

    private async Task<string> GetChatResponseAsync(
        MultipleChatCommand request,
        ModelType modelType,
        CancellationToken cancellationToken)
    {
        var serviceId = modelType.GetModelByType();
        var chatProvider = kernel.GetRequiredService<IChatCompletionService>(serviceId);
        
        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage(request.SystemBehaviourType.GetSystemPromptByBehaviour());
        chatHistory.AddUserMessage(UserBehaviourType.Default.GetUserPromptByBehaviour(request.Question));

        var chatResult = await chatProvider.GetChatMessageContentAsync(
            chatHistory, 
            cancellationToken: cancellationToken);

        return chatResult.Content ?? "No Response";
    }

    private async Task<AnswerEntity> CreateAnswerAsync(
        QuestionEntity question,
        ModelType modelType,
        string response,
        CancellationToken cancellationToken)
    {
        var answer = new AnswerEntity
        {
            Text = response,
            ModelType = modelType,
            AnswerType = AnswerType.Default,
            Question = question
        };

        await dbContext.Answers
                       .AddAsync(answer, cancellationToken);
        return answer;
    }
}