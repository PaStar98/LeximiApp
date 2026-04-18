using Leximi.Domain.Entities;
using Leximi.Application.Interfaces.Persistence;

namespace Leximi.Infrastructure.Persistence.Repositories;

public class AnswerRepository(LeximiDbContext context) : Repository<Answer>(context), IAnswerRepository
{
}
