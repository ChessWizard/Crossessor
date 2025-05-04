using System;
using System.ComponentModel.DataAnnotations.Schema;
using Crossessor.API.Domain.Entities.Common;

namespace Crossessor.API.Domain.Entities;

public class EvaluationEntity: AuditEntity<Guid>
{
    public byte Accuracy { get; set; }

    public byte Completeness { get; set; }
    
    public byte Clarity { get; set; }

    public byte Neutrality { get; set; }
    
    public double OverallScore { get; set; }

    public Guid AnswerId { get; set; }

    public AnswerEntity Answer { get; set; }
    
    public Guid? TargetAnswerId { get; set; }

    public AnswerEntity? TargetAnswer { get; set; }
    
    [NotMapped]
    public override Guid CreatedByUserId { get; set; }
    
    [NotMapped]
    public override Guid? ModifiedByUserId { get; set; }
    
    [NotMapped]
    public override Guid? DeletedByUserId { get; set; }
}