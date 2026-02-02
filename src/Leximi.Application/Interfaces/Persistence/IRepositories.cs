using Leximi.Domain.Entities;

namespace Leximi.Application.Interfaces.Persistence;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
    Task SaveChangesAsync();
}

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
}

public interface ICategoryRepository : IRepository<Category>
{
}

public interface ILearningSetRepository : IRepository<LearningSet>
{
    Task<LearningSet?> GetWithItemsAsync(Guid id);
}

public interface IAttemptRepository : IRepository<LearningSetAttempt>
{
    Task<IEnumerable<LearningSetAttempt>> GetByUserIdAsync(Guid userId);
}
