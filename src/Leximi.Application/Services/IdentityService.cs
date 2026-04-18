using Leximi.Application.DTOs;
using Leximi.Application.Interfaces.Persistence;
using Leximi.Application.Interfaces.Services;
using Leximi.Application.Interfaces.Authentication;
using Leximi.Domain.Entities;

namespace Leximi.Application.Services;

public class IdentityService(IUserRepository userRepository, IPasswordHasher passwordHasher, IJwtProvider jwtProvider) : IIdentityService
{
    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        var existingUser = await userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null) throw new Exception("User already exists");

        var user = new User
        {
            Email = request.Email,
            Username = request.Username,
            PasswordHash = passwordHasher.Hash(request.Password)
        };

        await userRepository.AddAsync(user);
        await userRepository.SaveChangesAsync();

        var token = jwtProvider.GenerateToken(user);
        return new AuthResponseDto(token, user.Username, user.Email);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        var user = await userRepository.GetByEmailAsync(request.Email);
        if (user == null || !passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new Exception("Invalid credentials");

        var token = jwtProvider.GenerateToken(user);
        return new AuthResponseDto(token, user.Username, user.Email);
    }

    public async Task<UserDto> GetCurrentUserAsync(Guid userId)
    {
        var user = await userRepository.GetByIdAsync(userId);
        if (user == null) throw new Exception("User not found");

        return new UserDto(user.Id, user.Username, user.Email);
    }
}
