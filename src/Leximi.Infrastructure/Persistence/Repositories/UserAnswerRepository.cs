using Leximi.Domain.Entities;
using Leximi.Application.Interfaces.Persistence;

namespace Leximi.Infrastructure.Persistence.Repositories;

public class UserAnswerRepository(LeximiDbContext context) : Repository<UserAnswer>(context), IUserAnswerRepository
{
}
