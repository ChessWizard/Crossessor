using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Crossessor.API.Domain.Entities.Common;

namespace Crossessor.API.Domain.Entities;

public class QuestionEntity: AuditEntity<Guid>
{
    public string Text { get; set; }

    public ICollection<AnswerEntity> Answers { get; set; }
    
    [NotMapped]
    public override Guid? ModifiedByUserId { get; set; }
    
    [NotMapped]
    public override DateTimeOffset? ModifiedDate { get; set; }
}