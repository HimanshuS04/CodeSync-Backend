using CodeSync.NotificationService.Data;
using CodeSync.NotificationService.Interfaces;
using CodeSync.NotificationService.Repositories;
using CodeSync.NotificationService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// PostgreSQL
builder.Services.AddDbContext<NotificationDbContext>(
    options => options.UseNpgsql(
        builder.Configuration
            .GetConnectionString("DefaultConnection")));

// Services
builder.Services.AddScoped<INotificationRepository,
    NotificationRepository>();
builder.Services.AddScoped<INotificationService,
    NotificationServiceImpl>();

// JWT
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
                ValidIssuer =
                    builder.Configuration["JwtSettings:Issuer"],
                ValidAudience =
                    builder.Configuration["JwtSettings:Audience"],
                IssuerSigningKey =
                    new SymmetricSecurityKey(key)
            };
    });

builder.Services.AddAuthorization();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CodeSync - Notification Service",
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
    service = "CodeSync Notification Service",
    timestamp = DateTime.UtcNow
}));
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider
        .GetRequiredService<NotificationDbContext>();
    db.Database.Migrate();
}

app.Run();