using Crossessor.API.Application.Interfaces.Entity;

namespace Crossessor.API.Domain.Entities.Common;

public class BaseEntity<T> : IEntity<T>
{
    public T Id { get; set; }
}