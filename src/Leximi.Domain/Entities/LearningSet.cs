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


