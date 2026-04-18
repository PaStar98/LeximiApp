using Leximi.Domain.Common;

namespace Leximi.Domain.Entities;

public class User : BaseEntity
{
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public required string Username { get; set; }
    public string? FullName { get; set; }
    
    public virtual ICollection<LearningSet> OwnedSets { get; set; } = new List<LearningSet>();
    public virtual ICollection<LearningSetAttempt> Attempts { get; set; } = new List<LearningSetAttempt>();
}
