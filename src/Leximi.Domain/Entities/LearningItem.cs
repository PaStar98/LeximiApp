using Leximi.Domain.Common;

namespace Leximi.Domain.Entities;

public class LearningItem : BaseEntity
{
    public Guid LearningSetId { get; set; }
    public virtual LearningSet LearningSet { get; set; } = null!;
    
    public string? QuestionContent { get; set; }
    public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();

    public string? FlashcardFront { get; set; }
    public string? FlashcardBack { get; set; }
}
