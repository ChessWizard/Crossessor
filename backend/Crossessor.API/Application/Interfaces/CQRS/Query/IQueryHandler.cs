using Crossessor.API.Application.Interfaces.CQRS.Query;
using MediatR;

namespace Crossessor.API.Application.Interfaces.CQRS.Query;

public interface IQueryHandler<in TQuery>
    : IQueryHandler<TQuery, Unit>
    where TQuery : IQuery<Unit>
{
}

public interface IQueryHandler<in TQuery, TResponse>
    : IRequestHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
    where TResponse : notnull
{
}