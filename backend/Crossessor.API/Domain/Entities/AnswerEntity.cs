using System.ComponentModel.DataAnnotations.Schema;
using Crossessor.API.Domain.Entities.Common;
using Crossessor.API.Domain.Enums;

namespace Crossessor.API.Domain.Entities;

public class AnswerEntity: AuditEntity<Guid>
{
    public string Text { get; set; }
    
    public ModelType ModelType { get; set; }

    public AnswerType AnswerType { get; set; }
    
    public Guid QuestionId { get; set; }

    public QuestionEntity Question { get; set; }
    
    [InverseProperty("Answer")]
    public ICollection<EvaluationEntity> Evaluations { get; set; }
    
    [InverseProperty("TargetAnswer")]
    public ICollection<EvaluationEntity> TargetEvaluations { get; set; }

    [NotMapped]
    public override Guid CreatedByUserId { get; set; }
    
    [NotMapped]
    public override Guid? ModifiedByUserId { get; set; }

    [NotMapped]
    public override DateTimeOffset? ModifiedDate { get; set; }
    
    [NotMapped]
    public override Guid? DeletedByUserId { get; set; }
}