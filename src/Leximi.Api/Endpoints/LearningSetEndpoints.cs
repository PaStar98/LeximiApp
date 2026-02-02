using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Leximi.Application.Interfaces.Services;
using Leximi.Application.DTOs;
using System.Security.Claims;

namespace Leximi.Api.Endpoints;

public static class LearningSetEndpoints
{
    public static void MapLearningSetEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/sets");

        group.MapGet("/{id}", async (Guid id, ILearningSetService service) =>
        {
            var set = await service.GetSetByIdAsync(id);
            return Results.Ok(set);
        });

        group.MapGet("/category/{categoryId}", async (Guid categoryId, ILearningSetService service) =>
        {
            var sets = await service.GetSetsByCategoryAsync(categoryId);
            return Results.Ok(sets);
        });

        group.MapPost("/", async (CreateLearningSetDto request, ClaimsPrincipal user, ILearningSetService service) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var set = await service.CreateSetAsync(request, userId);
            return Results.Ok(set);
        }).RequireAuthorization();
    }
}
