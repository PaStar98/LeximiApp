using Leximi.Domain.Entities;
using Leximi.Application.Interfaces.Persistence;

namespace Leximi.Infrastructure.Persistence.Repositories;

public class CategoryRepository(LeximiDbContext context) : Repository<Category>(context), ICategoryRepository
{
}
