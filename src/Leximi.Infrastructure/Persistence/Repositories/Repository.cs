using Microsoft.EntityFrameworkCore;
using Leximi.Application.Interfaces.Persistence;
using Leximi.Domain.Common;

namespace Leximi.Infrastructure.Persistence.Repositories;

public class Repository<T>(LeximiDbContext context) : IRepository<T> where T : BaseEntity
{
    protected readonly LeximiDbContext _context = context;
    protected readonly DbSet<T> _dbSet = context.Set<T>();

    public async Task<T?> GetByIdAsync(Guid id) => await _dbSet.FindAsync(id);

    public async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();

    public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);

    public void Update(T entity) => _dbSet.Update(entity);

    public void Delete(T entity) => _dbSet.Remove(entity);

    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();

    public void ClearTracker() => _context.ChangeTracker.Clear();
}
