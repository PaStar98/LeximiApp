using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Leximi.Application.Interfaces.Services;
using Leximi.Application.DTOs;
using System.Security.Claims;

namespace Leximi.Api.Endpoints;

public static class AttemptEndpoints
{
    public static void MapAttemptEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/attempts").RequireAuthorization();

        group.MapPost("/start/{setId}", async (Guid setId, ClaimsPrincipal user, IAttemptService service) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var attempt = await service.StartAttemptAsync(setId, userId);
            return Results.Ok(attempt);
        });

        group.MapPost("/{id}/answer", async (Guid id, SubmitAnswerDto request, IAttemptService service) =>
        {
            var attempt = await service.SubmitAnswerAsync(id, request);
            return Results.Ok(attempt);
        });

        group.MapPost("/{id}/finish", async (Guid id, IAttemptService service) =>
        {
            var attempt = await service.FinishAttemptAsync(id);
            return Results.Ok(attempt);
        });

        group.MapGet("/history", async (ClaimsPrincipal user, IAttemptService service) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var history = await service.GetUserHistoryAsync(userId);
            return Results.Ok(history);
        });
    }
}
