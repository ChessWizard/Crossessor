using Crossessor.API.Application.Models.Result;
using MediatR;

namespace Crossessor.API.Application.Interfaces.CQRS.Command;

public interface ICommand<out TResponse> : IRequest<TResponse>
{
}

public interface ICommand : ICommand<Result<Unit>>
{
}