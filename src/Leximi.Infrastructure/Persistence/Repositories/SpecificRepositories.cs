using Microsoft.EntityFrameworkCore;
using Leximi.Domain.Entities;
using Leximi.Application.Interfaces.Persistence;

namespace Leximi.Infrastructure.Persistence.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(LeximiDbContext context) : base(context) { }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted);
    }
}

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(LeximiDbContext context) : base(context) { }
}

public class LearningSetRepository : Repository<LearningSet>, ILearningSetRepository
{
    public LearningSetRepository(LeximiDbContext context) : base(context) { }

    public async Task<LearningSet?> GetWithItemsAsync(Guid id)
    {
        return await _dbSet
            .Include(s => s.Items.Where(i => !i.IsDeleted))
                .ThenInclude(i => i.Answers.Where(a => !a.IsDeleted))
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
    }

    public async Task<LearningSet?> GetWithItemsForUpdateAsync(Guid id)
    {
        // Load ONLY non-deleted items and answers.
        // This is the key: if soft-deleted entities are never loaded into the tracker,
        // there's nothing to detach, and EF's DetectChanges can't re-track them.
        return await _dbSet
            .Include(s => s.Items.Where(i => !i.IsDeleted))
                .ThenInclude(i => i.Answers.Where(a => !a.IsDeleted))
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
    }
}

public class AnswerRepository : Repository<Answer>, IAnswerRepository
{
    public AnswerRepository(LeximiDbContext context) : base(context) { }
}

public class AttemptRepository : Repository<LearningSetAttempt>, IAttemptRepository
{
    public AttemptRepository(LeximiDbContext context) : base(context) { }

    public async Task<IEnumerable<LearningSetAttempt>> GetByUserIdAsync(Guid userId)
    {
        return await _dbSet
            .Include(a => a.LearningSet)
                .ThenInclude(s => s.Items.Where(i => !i.IsDeleted))
            .Where(a => a.UserId == userId && !a.IsDeleted)
            .OrderByDescending(a => a.StartedAt)
            .ToListAsync();
    }

    public async Task<LearningSetAttempt?> GetWithUserAnswersAsync(Guid id)
    {
        return await _dbSet
            .AsSplitQuery()
            .Include(a => a.LearningSet)
            .Include(a => a.UserAnswers.Where(ua => !ua.IsDeleted))
            .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);
    }

    public async Task<int> CountCorrectAnswersAsync(Guid attemptId)
    {
        return await _context.UserAnswers.CountAsync(ua => ua.AttemptId == attemptId && ua.IsCorrect);
    }
}

public class UserAnswerRepository : Repository<UserAnswer>, IUserAnswerRepository
{
    public UserAnswerRepository(LeximiDbContext context) : base(context) { }
}
