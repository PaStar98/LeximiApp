using Microsoft.Extensions.DependencyInjection;
using Leximi.Application.Interfaces.Services;
using Leximi.Application.Services;

namespace Leximi.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ILearningSetService, LearningSetService>();
        services.AddScoped<IAttemptService, AttemptService>();

        return services;
    }
}
