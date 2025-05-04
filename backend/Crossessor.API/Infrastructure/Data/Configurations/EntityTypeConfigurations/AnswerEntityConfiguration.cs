using Crossessor.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crossessor.API.Infrastructure.Data.Configurations.EntityTypeConfigurations;

public class AnswerEntityConfiguration : AuditEntityTypeConfiguration<AnswerEntity>
{
    public void Configure(EntityTypeBuilder<AnswerEntity> builder)
    {
        builder.ToTable("Answers");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Text)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(a => a.ModelType)
            .IsRequired();

        builder.Property(a => a.AnswerType)
            .IsRequired();

        builder.HasOne(a => a.Question)
            .WithMany(q => q.Answers)
            .HasForeignKey(a => a.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.Evaluations)
            .WithOne(e => e.Answer)
            .HasForeignKey(e => e.AnswerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.TargetEvaluations)
            .WithOne(e => e.TargetAnswer)
            .HasForeignKey(e => e.TargetAnswerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}