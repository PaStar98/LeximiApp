using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Leximi.Infrastructure.Authentication;
using Leximi.Application.Interfaces.Authentication;

namespace Leximi.Api.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddAuthConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IJwtProvider, JwtProvider>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();

        var secret = configuration["Jwt:Secret"] ?? "a_very_long_and_secure_secret_key_for_development";
        var key = Encoding.UTF8.GetBytes(secret);

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };
            });

        services.AddAuthorization();
        return services;
    }
}
