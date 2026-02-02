using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Leximi.Application.Interfaces.Services;
using Leximi.Application.DTOs;

namespace Leximi.Api.Endpoints;

public static class CategoryEndpoints
{
    public static void MapCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/categories");

        group.MapGet("/", async (ICategoryService service) =>
        {
            var categories = await service.GetAllCategoriesAsync();
            return Results.Ok(categories);
        });

        group.MapPost("/", async (CreateCategoryDto request, ICategoryService service) =>
        {
            var category = await service.CreateCategoryAsync(request);
            return Results.Ok(category);
        }).RequireAuthorization(); // Add admin check later
    }
}
