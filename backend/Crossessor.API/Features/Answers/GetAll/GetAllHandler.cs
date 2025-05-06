using System.Net;
using Crossessor.API.Application.Interfaces.CQRS.Query;
using Crossessor.API.Application.Models.Result;
using Crossessor.API.Application.Models.Result.Paging;
using Crossessor.API.Application.Utilities.Helpers;
using Crossessor.API.Domain.Entities;
using Crossessor.API.Domain.Enums;
using Crossessor.API.Features.Common.Models.Response;
using Crossessor.API.Infrastructure.Data.Context;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Crossessor.API.Features.Answers.GetAll;

public record GetAllQuery(
    Guid? QuestionId,
    AnswerType? AnswerType,
    ModelType? ModelType,
    int PageNumber = 1,
    int PageSize = 20) : IQuery<PagingResult<List<AnswerResponse>>>;

public class GetAllHandler(CrossessorDbContext dbContext) : IQueryHandler<GetAllQuery, PagingResult<List<AnswerResponse>>>
{
    public async Task<PagingResult<List<AnswerResponse>>> Handle(
        GetAllQuery request,
        CancellationToken cancellationToken)
    {
        var query = BuildQuery(request);
        var (answers, totalCount) = await GetPaginatedResultsAsync(query, request, cancellationToken);

        if (answers.Count == 0)
        {
            return PagingResult<List<AnswerResponse>>.Error(
                "No answers found",
                (int)HttpStatusCode.NotFound);
        }

        var mappedAnswers = answers.Adapt<List<AnswerResponse>>();
        var pagingMetaData = CreatePagingMetaData(request, totalCount);

        return PagingResult<List<AnswerResponse>>.Success(
            mappedAnswers,
            pagingMetaData,
            (int)HttpStatusCode.OK);
    }

    private IQueryable<AnswerEntity> BuildQuery(GetAllQuery request)
    {
        var query = dbContext.Answers.AsNoTracking();

        if (request.QuestionId.HasValue)
        {
            query = query.Where(q => q.QuestionId == request.QuestionId.Value);
        }

        if (request.AnswerType is not null)
        {
            query = query.Where(q => q.AnswerType == request.AnswerType);
        }

        if (request.ModelType is not null)
        {
            query = query.Where(q => q.ModelType == request.ModelType);
        }

        return query;
    }

    private static async Task<(List<AnswerEntity> Answers, int TotalCount)> GetPaginatedResultsAsync(
        IQueryable<AnswerEntity> query,
        GetAllQuery request,
        CancellationToken cancellationToken)
    {
        var answers = await query
            .Paginate(request.PageNumber, request.PageSize)
            .ToListAsync(cancellationToken);

        var totalCount = await query.CountAsync(cancellationToken);

        return (answers, totalCount);
    }

    private static PagingMetaData CreatePagingMetaData(GetAllQuery request, int totalCount)
    {
        return new PagingMetaData(
            request.PageSize,
            request.PageNumber,
            totalCount
        );
    }
}