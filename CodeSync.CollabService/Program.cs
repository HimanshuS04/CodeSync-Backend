using CodeSync.CollabService.Data;
using CodeSync.CollabService.Hubs;
using CodeSync.CollabService.Interfaces;
using CodeSync.CollabService.OT;
using CodeSync.CollabService.Repositories;
using CodeSync.CollabService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. PostgreSQL
builder.Services.AddDbContext<CollabDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration
            .GetConnectionString("DefaultConnection")));

// 2. Redis + Services
builder.Services.AddSingleton<IRedisService, RedisService>();
builder.Services.AddSingleton<OTService>();
builder.Services.AddScoped<ICollabRepository,CollabRepository>();
builder.Services.AddScoped<ICollabService,CollabServiceImpl>();
builder.Services.AddHttpClient<INotificationClient, NotificationClient>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHostedService<SessionCleanupService>();

// 3. JWT
var key = Encoding.UTF8.GetBytes(
    builder.Configuration["JwtSettings:SecretKey"]!);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration[
                    "JwtSettings:Issuer"],
                ValidAudience = builder.Configuration[
                    "JwtSettings:Audience"],
                IssuerSigningKey =
                    new SymmetricSecurityKey(key)
            };

        // Allow JWT via query string for SignalR
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request
                    .Query["access_token"];
                var path = context.HttpContext
                    .Request.Path;
                if (!string.IsNullOrEmpty(accessToken)
                    && path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// 4. SignalR
builder.Services.AddSignalR();

// 5. CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
        policy.WithOrigins(
            "http://localhost:4200",
            "https://code-sync-frontend-tau.vercel.app"
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

// 6. Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CodeSync - Collab Service",
        Version = "v1"
    });
    c.AddSecurityDefinition("Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter token only"
        });
    c.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("AllowAngular");
app.UseAuthentication();
app.UseAuthorization();
app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    service = "CodeSync Collab Service",
    timestamp = DateTime.UtcNow
}));
app.MapControllers();

// Map SignalR Hub
app.MapHub<CollabHub>("/hubs/collab");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider
        .GetRequiredService<CollabDbContext>();
    db.Database.Migrate();
}

app.Run();