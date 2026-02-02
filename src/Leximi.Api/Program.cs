using Leximi.Infrastructure.Persistence;
using Leximi.Application;
using Leximi.Api.Extensions;
using Leximi.Api.Endpoints;
using Leximi.Api.Middlewares;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Leximi API", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer",
        Description = "Enter: Bearer {token}"
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer", document), []
        }
    });
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
                      ?? "Server=(localdb)\\mssqllocaldb;Database=LeximiDb;Trusted_Connection=True;MultipleActiveResultSets=true";

builder.Services.AddInfrastructurePersistence(connectionString);
builder.Services.AddApplication();
builder.Services.AddAuthConfiguration(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Map Endpoints
app.MapAuthEndpoints();
app.MapCategoryEndpoints();
app.MapLearningSetEndpoints();
app.MapAttemptEndpoints();

app.Run();
