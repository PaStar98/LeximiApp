using Leximi.Domain.Common;

namespace Leximi.Domain.Entities;

public class UserAnswer : BaseEntity
{
    public Guid AttemptId { get; set; }
    public virtual LearningSetAttempt Attempt { get; set; } = null!;
    
    public Guid LearningItemId { get; set; }
    public virtual LearningItem LearningItem { get; set; } = null!;
    
    public Guid? AnswerId { get; set; }
    public virtual Answer? Answer { get; set; }
    
    public string? ProvidedText { get; set; }
    public bool IsCorrect { get; set; }
}
