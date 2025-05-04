namespace Crossessor.API.Application.Interfaces.Entity;

public interface IEntity<T> : IEntity
{
    T Id { get; set; }
}