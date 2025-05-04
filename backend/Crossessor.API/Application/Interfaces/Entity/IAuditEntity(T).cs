namespace Crossessor.API.Application.Interfaces.Entity;

public interface IAuditEntity<T> 
    : IEntity<T>, ICreatedOn, IModifiedOn, IDeletedOn
{
}
