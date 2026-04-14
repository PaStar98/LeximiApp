using Leximi.Domain.Common;
using Leximi.Domain.Enums;

namespace Leximi.Domain.Entities;

public class LearningSet : BaseEntity
{
    public required string Title { get; set; }
    public string? Description { get; set; }
    public SetType Type { get; set; }
    
    public Guid OwnerId { get; set; }
    public virtual User Owner { get; set; } = null!;
    
    public Guid CategoryId { get; set; }
    public virtual Category Category { get; set; } = null!;
    
    public virtual ICollection<LearningItem> Items { get; set; } = new List<LearningItem>();
}

public class LearningItem : BaseEntity
{
    public Guid LearningSetId { get; set; }
    public virtual LearningSet LearningSet { get; set; } = null!;
    
    public string? QuestionContent { get; set; }
    public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();

    public string? FlashcardFront { get; set; }
    public string? FlashcardBack { get; set; }
}

public class Answer : BaseEntity
{
    public Guid LearningItemId { get; set; }
    public virtual LearningItem LearningItem { get; set; } = null!;
    
    public required string Content { get; set; }
    public bool IsCorrect { get; set; }
}

