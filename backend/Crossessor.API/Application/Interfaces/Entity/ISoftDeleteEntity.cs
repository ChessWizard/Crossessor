namespace Crossessor.API.Application.Interfaces.Entity;

public interface ISoftDeleteEntity
{
    bool IsDeleted { get; set; }
}