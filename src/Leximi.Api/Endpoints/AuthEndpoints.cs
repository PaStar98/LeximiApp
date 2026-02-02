using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Leximi.Application.Interfaces.Services;
using Leximi.Application.DTOs;
using System.Security.Claims;

namespace Leximi.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth");

        group.MapPost("/register", async (RegisterRequestDto request, IIdentityService service) =>
        {
            var response = await service.RegisterAsync(request);
            return Results.Ok(response);
        });

        group.MapPost("/login", async (LoginRequestDto request, IIdentityService service) =>
        {
            var response = await service.LoginAsync(request);
            return Results.Ok(response);
        });

        group.MapGet("/me", async (ClaimsPrincipal user, IIdentityService service) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var response = await service.GetCurrentUserAsync(userId);
            return Results.Ok(response);
        }).RequireAuthorization();
    }
}
