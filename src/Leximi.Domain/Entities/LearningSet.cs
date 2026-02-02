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
    
    public virtual Question? Question { get; set; }
    public virtual Flashcard? Flashcard { get; set; }
}

public class Question : BaseEntity
{
    public Guid LearningItemId { get; set; }
    public virtual LearningItem LearningItem { get; set; } = null!;
    
    public required string Content { get; set; }
    public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();
}

public class Answer : BaseEntity
{
    public Guid QuestionId { get; set; }
    public virtual Question Question { get; set; } = null!;
    
    public required string Content { get; set; }
    public bool IsCorrect { get; set; }
}

public class Flashcard : BaseEntity
{
    public Guid LearningItemId { get; set; }
    public virtual LearningItem LearningItem { get; set; } = null!;
    
    public required string Front { get; set; }
    public required string Back { get; set; }
}
