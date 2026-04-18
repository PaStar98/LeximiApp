using System.Net;
using System.Net.Http.Json;
using Leximi.Application.DTOs;
using Leximi.Tests.Fixtures;
using Moq;
using FluentAssertions;

namespace Leximi.Tests.Endpoints;

public class AttemptEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AttemptEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAttemptById_ShouldReturnOk()
    {
        // Arrange
        var attemptId = Guid.NewGuid();
        var attemptDto = new AttemptDto(attemptId, Guid.NewGuid(), DateTime.UtcNow, null, 0);

        _factory.AttemptServiceMock
            .Setup(s => s.GetAttemptByIdAsync(attemptId))
            .ReturnsAsync(attemptDto);

        // Act
        var response = await _client.GetAsync($"/api/attempts/{attemptId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AttemptDto>();
        result.Should().BeEquivalentTo(attemptDto);
    }

    [Fact]
    public async Task StartAttempt_ShouldReturnOk_WhenAuthenticated()
    {
        // Arrange
        var setId = Guid.NewGuid();
        var attemptDto = new AttemptDto(Guid.NewGuid(), setId, DateTime.UtcNow, null, 0);

        _factory.AttemptServiceMock
            .Setup(s => s.StartAttemptAsync(setId, It.IsAny<Guid>()))
            .ReturnsAsync(attemptDto);

        // Act
        var response = await _client.PostAsync($"/api/attempts/start/{setId}", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AttemptDto>();
        result.Should().BeEquivalentTo(attemptDto);
    }

    [Fact]
    public async Task SubmitAnswer_ShouldReturnOk()
    {
        // Arrange
        var attemptId = Guid.NewGuid();
        var request = new SubmitAnswerDto(Guid.NewGuid(), Guid.NewGuid(), "Answer");
        var attemptDto = new AttemptDto(attemptId, Guid.NewGuid(), DateTime.UtcNow, null, 0);

        _factory.AttemptServiceMock
            .Setup(s => s.SubmitAnswerAsync(attemptId, It.IsAny<SubmitAnswerDto>()))
            .ReturnsAsync(attemptDto);

        // Act
        var response = await _client.PostAsJsonAsync($"/api/attempts/{attemptId}/answer", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AttemptDto>();
        result.Should().BeEquivalentTo(attemptDto);
    }

    [Fact]
    public async Task FinishAttempt_ShouldReturnOk()
    {
        // Arrange
        var attemptId = Guid.NewGuid();
        var attemptDto = new AttemptDto(attemptId, Guid.NewGuid(), DateTime.UtcNow, DateTime.UtcNow.AddMinutes(5), 80);

        _factory.AttemptServiceMock
            .Setup(s => s.FinishAttemptAsync(attemptId))
            .ReturnsAsync(attemptDto);

        // Act
        var response = await _client.PostAsync($"/api/attempts/{attemptId}/finish", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AttemptDto>();
        result.Should().BeEquivalentTo(attemptDto);
    }

    [Fact]
    public async Task GetHistory_ShouldReturnOk_WhenAuthenticated()
    {
        // Arrange
        var history = new List<AttemptHistoryDto>
        {
            new AttemptHistoryDto(Guid.NewGuid(), Guid.NewGuid(), "Set 1", "Flashcards", DateTime.UtcNow, DateTime.UtcNow, 100, 100)
        };

        _factory.AttemptServiceMock
            .Setup(s => s.GetUserHistoryAsync(It.IsAny<Guid>()))
            .ReturnsAsync(history);

        // Act
        var response = await _client.GetAsync("/api/attempts/history");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<AttemptHistoryDto>>();
        result.Should().BeEquivalentTo(history);
    }
}
