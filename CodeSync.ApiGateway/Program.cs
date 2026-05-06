using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile(
    "ocelot.json",
    optional: false,
    reloadOnChange: true);

builder.Configuration.AddJsonFile(
    $"ocelot.{builder.Environment.EnvironmentName}.json",
    optional: true,
    reloadOnChange: true);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy
            .WithOrigins(
                "http://localhost:4200",
                "https://code-sync-frontend-tau.vercel.app")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

builder.Services.AddOcelot();

var app = builder.Build();

app.UseCors("AllowAll");

app.MapMethods("/",
    new[] { "GET", "HEAD" },
    () => Results.Ok(new
    {
        status = "healthy",
        service = "CodeSync API Gateway"
    }));

app.MapMethods("/health",
    new[] { "GET", "HEAD" },
    () => Results.Ok(new
    {
        status = "healthy"
    }));

await app.UseOcelot();

app.Run();