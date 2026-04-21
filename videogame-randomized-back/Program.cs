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
using videogame_randomized_back.Infrastructure;
using videogame_randomized_back.Mappers;
using videogame_randomized_back.Models;
using videogame_randomized_back.Services;

var rootEnvPath = ResolveRootEnvPath();

if (rootEnvPath is not null)
{
    Env.NoClobber().Load(rootEnvPath);
}

var builder = WebApplication.CreateBuilder(args);

var dbHost = GetEnvOrDefault("DB_HOST", "localhost");
var dbName = GetEnvOrDefault("DB_NAME", "videogames");
var dbUser = GetEnvOrDefault("DB_USER", "root");
var dbPassword = GetEnvOrDefault("DB_PASSWORD", "password");
var connectionString = $"Server={dbHost};Database={dbName};Uid={dbUser};Pwd={dbPassword};";

var jwtSecret = GetEnvOrDefault("JWT_SECRET", "fallback-secret-key-min-32-characters-long!");
var jwtIssuer = GetEnvOrDefault("JWT_ISSUER", "videogame-randomizer");
var jwtAudience = GetEnvOrDefault("JWT_AUDIENCE", "videogame-randomizer-frontend");
var jwtExpirationMinutes = int.Parse(GetEnvOrDefault("JWT_EXPIRATION_MINUTES", "1440"));

var emailFromEmail = GetEnvOrDefault("EMAIL_FROM_EMAIL", "hello@example.com");
var emailFromName = GetEnvOrDefault("EMAIL_FROM_NAME", "VideoGame Randomizer");
var emailApiToken = GetEnvOrDefault("EMAIL_API_TOKEN", "");

var corsOriginsEnv = GetEnvOrDefault(
    "CORS_ALLOWED_ORIGINS",
    "http://localhost:5173,http://localhost:3000");
var allowedOrigins = string.IsNullOrWhiteSpace(corsOriginsEnv) 
    ? Array.Empty<string>() 
    : corsOriginsEnv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

static string? ResolveRootEnvPath()
{
    var candidatePaths = new[]
    {
        Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", ".env")),
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".env"))
    };

    return candidatePaths.FirstOrDefault(File.Exists);
}

static string GetEnvOrDefault(string key, string fallback)
{
    var value = Environment.GetEnvironmentVariable(key);
    return string.IsNullOrWhiteSpace(value) ? fallback : value;
}

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
builder.Services.AddHttpClient("rawg", client =>
{
    client.BaseAddress = new Uri("https://api.rawg.io/api/");
    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
});
builder.Services.AddHttpClient("googleTranslate", client =>
{
    client.BaseAddress = new Uri("https://translation.googleapis.com/");
    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
});
builder.Services.AddHttpClient("youtube", client =>
{
    client.BaseAddress = new Uri("https://www.googleapis.com/youtube/v3/");
    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
});

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
builder.Services.AddScoped<RawgService>();
builder.Services.AddScoped<TranslateService>();
builder.Services.AddScoped<YoutubeService>();
builder.Services.AddScoped<GameMapper>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateGameDto>();
builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

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

// Apply pending migrations automatically on startup (skipped in "Testing" for integration tests)
if (!app.Environment.IsEnvironment("Testing"))
{
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
}

app.UseExceptionHandler();
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
