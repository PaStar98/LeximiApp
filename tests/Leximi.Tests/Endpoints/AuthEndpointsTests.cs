using System.Net;
using System.Net.Http.Json;
using Leximi.Application.DTOs;
using Leximi.Tests.Fixtures;
using Moq;
using FluentAssertions;

namespace Leximi.Tests.Endpoints;

public class AuthEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AuthEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_ShouldReturnOk_WhenSuccessful()
    {
        // Arrange
        var request = new RegisterRequestDto("test@example.com", "TestUser", "Password123!");
        var responseDto = new AuthResponseDto("token", "TestUser", "test@example.com");
        
        _factory.IdentityServiceMock
            .Setup(s => s.RegisterAsync(It.IsAny<RegisterRequestDto>()))
            .ReturnsAsync(responseDto);

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        result.Should().BeEquivalentTo(responseDto);
    }

    [Fact]
    public async Task Login_ShouldReturnOk_WhenSuccessful()
    {
        // Arrange
        var request = new LoginRequestDto("test@example.com", "Password123!");
        var responseDto = new AuthResponseDto("token", "TestUser", "test@example.com");

        _factory.IdentityServiceMock
            .Setup(s => s.LoginAsync(It.IsAny<LoginRequestDto>()))
            .ReturnsAsync(responseDto);

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        result.Should().BeEquivalentTo(responseDto);
    }

    [Fact]
    public async Task GetMe_ShouldReturnOk_WhenAuthenticated()
    {
        // Arrange
        var userDto = new UserDto(Guid.NewGuid(), "test@example.com", "TestUser");

        _factory.IdentityServiceMock
            .Setup(s => s.GetCurrentUserAsync(It.IsAny<Guid>()))
            .ReturnsAsync(userDto);

        // Act
        var response = await _client.GetAsync("/api/auth/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<UserDto>();
        result.Should().BeEquivalentTo(userDto);
    }
}
