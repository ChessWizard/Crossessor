using Crossessor.API.Application.Interfaces.Entity;
using Crossessor.API.Infrastructure.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace Crossessor.API.Infrastructure.Data.Interceptors;

public partial class AuditEntitySaveChangeInterceptor
{
    private void UpdateAuidtFields(DbContext context)
    {
        var entries = context.ChangeTracker
            .Entries<IAuditEntity<Guid>>()
            .Where(e => e.State is EntityState.Added or 
                EntityState.Modified or 
                EntityState.Deleted);
        
        var currentTime = DateTimeOffset.UtcNow;
        
        // TODO: Üyelik sistemi gelirse burası contextAccessor'dan gelmeli
        var currentUserId = SeedConstants.DEFAULT_USER_ID;

        foreach (var entry in entries)
        {
            var auditEntity = entry.Entity;
            switch (entry.State)
            {
                case EntityState.Added:
                    auditEntity.CreatedDate = currentTime;
                    auditEntity.CreatedByUserId = Guid.Parse(currentUserId);
                    break;
                case EntityState.Modified:
                    auditEntity.ModifiedDate = currentTime;
                    auditEntity.ModifiedByUserId = Guid.Parse(currentUserId);
                    break;
                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    auditEntity.DeletedDate = currentTime;
                    auditEntity.IsDeleted = true;
                    auditEntity.DeletedByUserId = Guid.Parse(currentUserId);
                    break;
            }
        }
    }
}