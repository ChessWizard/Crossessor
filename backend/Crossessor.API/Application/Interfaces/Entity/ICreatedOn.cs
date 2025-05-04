namespace Crossessor.API.Application.Interfaces.Entity;

public interface ICreatedOn
{
    Guid CreatedByUserId { get; set; }
    
    DateTimeOffset CreatedDate { get; set; }
}
