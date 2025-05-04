using Crossessor.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Crossessor.API.Infrastructure.Data.Context;

public class CrossessorDbContext(DbContextOptions<CrossessorDbContext> contextOptions)
    : DbContext(contextOptions)
{
    public DbSet<QuestionEntity> Questions => Set<QuestionEntity>();
    
    public DbSet<AnswerEntity> Answers => Set<AnswerEntity>();
    
    public DbSet<EvaluationEntity> Evaluations => Set<EvaluationEntity>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
    }
}