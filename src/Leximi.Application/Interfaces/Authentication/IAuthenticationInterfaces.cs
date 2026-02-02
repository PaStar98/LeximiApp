using Leximi.Domain.Entities;

namespace Leximi.Application.Interfaces.Authentication;

public interface IJwtProvider
{
    string GenerateToken(User user);
}

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}
