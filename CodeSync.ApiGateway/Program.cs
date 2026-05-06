using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Load ocelot config based on environment
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
        policy.WithOrigins(
            "http://localhost:4200",
            "https://code-sync-frontend-tau.vercel.app",
            "https://code-sync-frontend-1o1aoj9ao-himanshus04s-projects.vercel.app"
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

builder.Services.AddOcelot();

var app = builder.Build();

app.UseCors("AllowAll");
// Health check endpoint
app.MapGet("/", () => Results.Ok(new
{
    status = "healthy",
    service = "CodeSync API Gateway",
    timestamp = DateTime.UtcNow
}));

app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy"
}));


await app.UseOcelot();

app.Run();