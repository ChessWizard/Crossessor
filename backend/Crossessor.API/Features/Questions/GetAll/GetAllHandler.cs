using System.Net;
using Crossessor.API.Application.Interfaces.CQRS.Query;
using Crossessor.API.Application.Models.Result;
using Crossessor.API.Application.Utilities.Helpers;
using Crossessor.API.Features.Questions.Common;
using Crossessor.API.Infrastructure.Data.Context;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Crossessor.API.Features.Questions.GetAll;

public record GetAllQuery(
    int PageNumber = 1,
    int PageSize = 20) : IQuery<Result<List<QuestionResponse>>>;
    
public class GetAllHandler(CrossessorDbContext dbContext) : IQueryHandler<GetAllQuery, Result<List<QuestionResponse>>>
{
    public async Task<Result<List<QuestionResponse>>> Handle(GetAllQuery request, CancellationToken cancellationToken)
    {
        var questions = await dbContext.Questions
            .AsNoTracking()
            .Paginate(request.PageNumber, request.PageSize)
            .ToListAsync(cancellationToken);
        
        if(questions.Count == 0)
            return Result<List<QuestionResponse>>.Error("No questions found", (int)HttpStatusCode.NotFound);
        
        var mappedQuestions = questions.Adapt<List<QuestionResponse>>();
        return Result<List<QuestionResponse>>.Success(mappedQuestions, (int)HttpStatusCode.OK);
    }
}