using Microsoft.EntityFrameworkCore;
using Leximi.Domain.Entities;
using Leximi.Application.Interfaces.Persistence;

namespace Leximi.Infrastructure.Persistence.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(LeximiDbContext context) : base(context) { }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
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
            .Include(s => s.Items)
                .ThenInclude(i => i.Question)
                    .ThenInclude(q => q.Answers)
            .Include(s => s.Items)
                .ThenInclude(i => i.Flashcard)
            .FirstOrDefaultAsync(s => s.Id == id);
    }
}

public class AttemptRepository : Repository<LearningSetAttempt>, IAttemptRepository
{
    public AttemptRepository(LeximiDbContext context) : base(context) { }

    public async Task<IEnumerable<LearningSetAttempt>> GetByUserIdAsync(Guid userId)
    {
        return await _dbSet
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.StartedAt)
            .ToListAsync();
    }
}
