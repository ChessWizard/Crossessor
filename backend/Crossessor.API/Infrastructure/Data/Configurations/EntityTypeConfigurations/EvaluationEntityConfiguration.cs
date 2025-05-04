using Crossessor.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crossessor.API.Infrastructure.Data.Configurations.EntityTypeConfigurations;

public class EvaluationEntityConfiguration : AuditEntityTypeConfiguration<EvaluationEntity>
{
    public void Configure(EntityTypeBuilder<EvaluationEntity> builder)
    {
        builder.ToTable("Evaluations");

        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Accuracy)
            .IsRequired();
            
        builder.Property(e => e.Completeness)
            .IsRequired();
            
        builder.Property(e => e.Clarity)
            .IsRequired();
            
        builder.Property(e => e.Neutrality)
            .IsRequired();
            
        builder.Property(e => e.OverallScore)
            .IsRequired();
        
        builder.HasOne(e => e.Answer)
            .WithMany(a => a.Evaluations)
            .HasForeignKey(e => e.AnswerId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(e => e.TargetAnswer)
            .WithMany(a => a.TargetEvaluations)
            .HasForeignKey(e => e.TargetAnswerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
} 