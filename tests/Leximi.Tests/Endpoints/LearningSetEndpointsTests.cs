using System.Net;
using System.Net.Http.Json;
using Leximi.Application.DTOs;
using Leximi.Tests.Fixtures;
using Moq;
using FluentAssertions;

namespace Leximi.Tests.Endpoints;

public class LearningSetEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public LearningSetEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetSetById_ShouldReturnOk()
    {
        // Arrange
        var setId = Guid.NewGuid();
        var detailsDto = new LearningSetDetailsDto(
            setId, 
            "Set 1", 
            "Description 1", 
            Guid.NewGuid(), 
            "Flashcards", 
            new List<LearningItemDto>(), 
            "Author 1");

        _factory.LearningSetServiceMock
            .Setup(s => s.GetSetByIdAsync(setId))
            .ReturnsAsync(detailsDto);

        // Act
        var response = await _client.GetAsync($"/api/sets/{setId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<LearningSetDetailsDto>();
        result.Should().BeEquivalentTo(detailsDto);
    }

    [Fact]
    public async Task GetSetsByCategory_ShouldReturnOk()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var sets = new List<LearningSetDto>
        {
            new LearningSetDto(Guid.NewGuid(), "Set 1", "Description 1", categoryId, "Flashcards", "Author 1")
        };

        _factory.LearningSetServiceMock
            .Setup(s => s.GetSetsByCategoryAsync(categoryId))
            .ReturnsAsync(sets);

        // Act
        var response = await _client.GetAsync($"/api/sets/category/{categoryId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<LearningSetDto>>();
        result.Should().BeEquivalentTo(sets);
    }

    [Fact]
    public async Task CreateSet_ShouldReturnOk_WhenAuthenticated()
    {
        // Arrange
        var request = new CreateLearningSetDto("New Set", "New Description", Guid.NewGuid(), "Flashcards", new List<UpdateLearningItemDto>());
        var responseDto = new LearningSetDto(Guid.NewGuid(), "New Set", "New Description", request.CategoryId, "Flashcards", "Author 1");

        _factory.LearningSetServiceMock
            .Setup(s => s.CreateSetAsync(It.IsAny<CreateLearningSetDto>(), It.IsAny<Guid>()))
            .ReturnsAsync(responseDto);

        // Act
        var response = await _client.PostAsJsonAsync("/api/sets", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<LearningSetDto>();
        result.Should().BeEquivalentTo(responseDto);
    }

    [Fact]
    public async Task UpdateSet_ShouldReturnOk_WhenAuthenticated()
    {
        // Arrange
        var setId = Guid.NewGuid();
        var request = new UpdateLearningSetDto("Updated Set", "Updated Description", "Flashcards", Guid.NewGuid(), new List<UpdateLearningItemDto>());
        var detailsDto = new LearningSetDetailsDto(
            setId, 
            "Updated Set", 
            "Updated Description", 
            request.CategoryId,
            "Flashcards", 
            new List<LearningItemDto>(), 
            "Author 1");

        _factory.LearningSetServiceMock
            .Setup(s => s.UpdateSetAsync(setId, It.IsAny<UpdateLearningSetDto>(), It.IsAny<Guid>()))
            .ReturnsAsync(detailsDto);

        // Act
        var response = await _client.PutAsJsonAsync($"/api/sets/{setId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<LearningSetDetailsDto>();
        result.Should().BeEquivalentTo(detailsDto);
    }
}
