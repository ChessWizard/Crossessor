using Crossessor.API.Application.Interfaces.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crossessor.API.Infrastructure.Data.Configurations.EntityTypeConfigurations;

public abstract class AuditEntityTypeConfiguration<T> 
    : IEntityTypeConfiguration<T> where T : 
    class, IAuditEntity<Guid>
{
    public virtual void Configure(EntityTypeBuilder<T> builder)
    {
        builder.Property(u => u.CreatedDate).IsRequired();
        
        builder.HasQueryFilter(u => !u.IsDeleted);
    }
}