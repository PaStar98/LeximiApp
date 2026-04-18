using Leximi.Domain.Common;

namespace Leximi.Domain.Entities;

public class LearningSetAttempt : BaseEntity
{
    public Guid UserId { get; set; }
    public virtual User User { get; set; } = null!;
    
    public Guid LearningSetId { get; set; }
    public virtual LearningSet LearningSet { get; set; } = null!;
    
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? FinishedAt { get; set; }
    public int Score { get; set; }
    
    public virtual ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
}


