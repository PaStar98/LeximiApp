using System.Net;
using System.Net.Http.Json;
using Leximi.Application.DTOs;
using Leximi.Tests.Fixtures;
using Moq;
using FluentAssertions;

namespace Leximi.Tests.Endpoints;

public class CategoryEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public CategoryEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAllCategories_ShouldReturnOk()
    {
        var categories = new List<CategoryDto>
        {
            new CategoryDto(Guid.NewGuid(), "Category 1", "Description 1"),
            new CategoryDto(Guid.NewGuid(), "Category 2", "Description 2")
        };

        _factory.CategoryServiceMock
            .Setup(s => s.GetAllCategoriesAsync())
            .ReturnsAsync(categories);

        var response = await _client.GetAsync("/api/categories");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<CategoryDto>>();
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(categories);
    }

    [Fact]
    public async Task CreateCategory_ShouldReturnOk_WhenAuthenticated()
    {
        var request = new CreateCategoryDto("New Category", "New Description");
        var categoryDto = new CategoryDto(Guid.NewGuid(), "New Category", "New Description");

        _factory.CategoryServiceMock
            .Setup(s => s.CreateCategoryAsync(It.IsAny<CreateCategoryDto>()))
            .ReturnsAsync(categoryDto);

        var response = await _client.PostAsJsonAsync("/api/categories", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<CategoryDto>();
        result.Should().BeEquivalentTo(categoryDto);
    }
}
