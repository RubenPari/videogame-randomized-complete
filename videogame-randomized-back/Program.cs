using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json.Serialization;
using DotNetEnv;
using videogame_randomized_back.Data;
using videogame_randomized_back.DTOs;
using videogame_randomized_back.Mappers;
using videogame_randomized_back.Models;
using videogame_randomized_back.Services;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

var dbHost = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
var dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? "videogames";
var dbUser = Environment.GetEnvironmentVariable("DB_USER") ?? "root";
var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "password";
var connectionString = $"Server={dbHost};Database={dbName};Uid={dbUser};Pwd={dbPassword};";

var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? "fallback-secret-key-min-32-characters-long!";
var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "videogame-randomizer";
var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "videogame-randomizer-frontend";
var jwtExpirationMinutes = int.Parse(Environment.GetEnvironmentVariable("JWT_EXPIRATION_MINUTES") ?? "1440");

var emailFromEmail = Environment.GetEnvironmentVariable("EMAIL_FROM_EMAIL") ?? "hello@example.com";
var emailFromName = Environment.GetEnvironmentVariable("EMAIL_FROM_NAME") ?? "VideoGame Randomizer";
var emailApiToken = Environment.GetEnvironmentVariable("EMAIL_API_TOKEN") ?? "";

var corsOriginsEnv = Environment.GetEnvironmentVariable("CORS_ALLOWED_ORIGINS") ?? "";
var allowedOrigins = string.IsNullOrWhiteSpace(corsOriginsEnv) 
    ? Array.Empty<string>() 
    : corsOriginsEnv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        connectionString,
        new MySqlServerVersion(new Version(8, 0, 0)),
        b => b.MigrationsAssembly("videogame-randomized-back")
    ));

// Add Identity
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// Configure JWT
var jwtSettings = new JwtSettings
{
    Secret = jwtSecret,
    Issuer = jwtIssuer,
    Audience = jwtAudience,
    ExpirationMinutes = jwtExpirationMinutes
};
builder.Services.Configure<JwtSettings>(options =>
{
    options.Secret = jwtSecret;
    options.Issuer = jwtIssuer;
    options.Audience = jwtAudience;
    options.ExpirationMinutes = jwtExpirationMinutes;
});

if (jwtSecret.Length < 32)
{
    throw new InvalidOperationException("JWT secret must be at least 32 characters long for production security.");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
    };
});

builder.Services.AddAuthorization();

// Add HttpClient for Mailtrap API
builder.Services.AddHttpClient();

// Configure Email settings
builder.Services.Configure<EmailSettings>(options =>
{
    options.FromEmail = emailFromEmail;
    options.FromName = emailFromName;
    options.ApiToken = emailApiToken;
});

// Add services to the container
builder.Services.AddScoped<GamesService>();
builder.Services.AddScoped<IDiscoveryLogService, DiscoveryLogService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<GameMapper>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateGameDto>();
builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Add Swagger for API documentation
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "VideoGame Randomizer API", Version = "v1" });
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
        else
        {
            policy.DisallowCredentials();
        }
    });
});

var app = builder.Build();

// Apply pending migrations automatically on startup (with retry for Cloud SQL proxy readiness)
try
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var retries = 5;
    for (var i = 0; i < retries; i++)
    {
        try
        {
            db.Database.Migrate();
            break;
        }
        catch (Exception ex) when (i < retries - 1)
        {
            Console.WriteLine($"Migration attempt {i + 1} failed: {ex.Message}. Retrying in 5s...");
            Thread.Sleep(5000);
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Migration failed after all retries: {ex.Message}");
    if (!app.Environment.IsProduction()) throw;
}

app.UseCors();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/", () => "Hello World!");

app.Run();
