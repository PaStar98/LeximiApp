using Microsoft.EntityFrameworkCore;
using Leximi.Domain.Entities;
using Leximi.Application.Interfaces.Persistence;

namespace Leximi.Infrastructure.Persistence.Repositories;

public class LearningSetRepository(LeximiDbContext context) : Repository<LearningSet>(context), ILearningSetRepository
{
    public async Task<LearningSet?> GetWithItemsAsync(Guid id)
    {
        return await _dbSet
            .Include(s => s.Owner)
            .Include(s => s.Items.Where(i => !i.IsDeleted))
                .ThenInclude(i => i.Answers.Where(a => !a.IsDeleted))
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
    }

    public async Task<LearningSet?> GetWithItemsForUpdateAsync(Guid id)
    {
        return await _dbSet
            .Include(s => s.Owner)
            .Include(s => s.Items.Where(i => !i.IsDeleted))
                .ThenInclude(i => i.Answers.Where(a => !a.IsDeleted))
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
    }

    public async Task<IEnumerable<LearningSet>> GetByCategoryAsync(Guid categoryId)
    {
        return await _dbSet
            .Include(s => s.Owner)
            .Where(s => s.CategoryId == categoryId && !s.IsDeleted)
            .ToListAsync();
    }
}
