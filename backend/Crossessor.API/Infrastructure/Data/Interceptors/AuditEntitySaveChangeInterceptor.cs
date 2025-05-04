using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Crossessor.API.Infrastructure.Data.Interceptors;

public partial class AuditEntitySaveChangeInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context is null) 
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        
        UpdateAuidtFields(context);        
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}