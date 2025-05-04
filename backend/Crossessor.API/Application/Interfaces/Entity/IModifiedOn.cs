namespace Crossessor.API.Application.Interfaces.Entity;

public interface IModifiedOn
{
    Guid? ModifiedByUserId { get; set; }
    
    DateTimeOffset? ModifiedDate { get; set; }
}