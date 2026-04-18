using Leximi.Domain.Common;

namespace Leximi.Domain.Entities;

public class Answer : BaseEntity
{
    public Guid LearningItemId { get; set; }
    public virtual LearningItem LearningItem { get; set; } = null!;
    
    public required string Content { get; set; }
    public bool IsCorrect { get; set; }
}
