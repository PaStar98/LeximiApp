using Leximi.Application.DTOs;
using Leximi.Application.Interfaces.Persistence;
using Leximi.Application.Interfaces.Services;
using Leximi.Application.Interfaces.Authentication;
using Leximi.Domain.Entities;

namespace Leximi.Application.Services;

public class IdentityService : IIdentityService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtProvider _jwtProvider;

    public IdentityService(IUserRepository userRepository, IPasswordHasher passwordHasher, IJwtProvider jwtProvider)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtProvider = jwtProvider;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null) throw new Exception("User already exists");

        var user = new User
        {
            Email = request.Email,
            Username = request.Username,
            PasswordHash = _passwordHasher.Hash(request.Password)
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        var token = _jwtProvider.GenerateToken(user);
        return new AuthResponseDto(token, user.Username, user.Email);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new Exception("Invalid credentials");

        var token = _jwtProvider.GenerateToken(user);
        return new AuthResponseDto(token, user.Username, user.Email);
    }

    public async Task<UserDto> GetCurrentUserAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) throw new Exception("User not found");

        return new UserDto(user.Id, user.Username, user.Email);
    }
}
