namespace Crossessor.API.Application.Interfaces.Entity;

public interface IDeletedOn : ISoftDeleteEntity
{ 
    Guid? DeletedByUserId { get; set; }
    
    DateTimeOffset? DeletedDate { get; set; }
}