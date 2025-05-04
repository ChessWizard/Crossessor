using Crossessor.API.Application.Interfaces.Entity;

namespace Crossessor.API.Domain.Entities.Common;

public class AuditEntity<T> : IAuditEntity<T>
{
    public T Id { get; set; }
    
    public virtual Guid CreatedByUserId { get; set; }
    
    public DateTimeOffset CreatedDate { get; set; }
    
    public virtual Guid? ModifiedByUserId { get; set; }
    
    public virtual DateTimeOffset? ModifiedDate { get; set; }
    
    public bool IsDeleted { get; set; }
    
    public virtual Guid? DeletedByUserId { get; set; }
    
    public DateTimeOffset? DeletedDate { get; set; }
}