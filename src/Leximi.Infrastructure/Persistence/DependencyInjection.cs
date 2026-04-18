using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Leximi.Application.Interfaces.Persistence;
using Leximi.Infrastructure.Persistence.Repositories;

namespace Leximi.Infrastructure.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructurePersistence(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<LeximiDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
            options.EnableSensitiveDataLogging();
        });

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ILearningSetRepository, LearningSetRepository>();
        services.AddScoped<IAttemptRepository, AttemptRepository>();
        services.AddScoped<IAnswerRepository, AnswerRepository>();
        services.AddScoped<IUserAnswerRepository, UserAnswerRepository>();
        
        return services;
    }
}
