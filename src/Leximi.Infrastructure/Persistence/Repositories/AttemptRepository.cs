using Microsoft.EntityFrameworkCore;
using Leximi.Domain.Entities;
using Leximi.Application.Interfaces.Persistence;

namespace Leximi.Infrastructure.Persistence.Repositories;

public class AttemptRepository(LeximiDbContext context) : Repository<LearningSetAttempt>(context), IAttemptRepository
{
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
            .FirstOrDefaultAsync(a => id == a.Id && !a.IsDeleted);
    }

    public async Task<int> CountCorrectAnswersAsync(Guid attemptId)
    {
        return await _context.UserAnswers.CountAsync(ua => ua.AttemptId == attemptId && ua.IsCorrect);
    }
}
