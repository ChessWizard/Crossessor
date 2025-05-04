namespace Crossessor.API.Application.Interfaces.Entity;

public interface IAuditEntity 
    : IEntity, ICreatedOn, IModifiedOn, IDeletedOn
{
}

