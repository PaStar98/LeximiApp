using Leximi.Domain.Common;

namespace Leximi.Domain.Entities;

public class Category : BaseEntity
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    
    public virtual ICollection<LearningSet> LearningSets { get; set; } = new List<LearningSet>();
}
