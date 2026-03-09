using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using videogame_randomized_back.Data;
using videogame_randomized_back.DTOs;
using videogame_randomized_back.Endpoints;
using videogame_randomized_back.Mappers;
using videogame_randomized_back.Services;
using videogame_randomized_back.Validators;

var builder = WebApplication.CreateBuilder(args);

// Add DbContext with MySQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=localhost;Database=videogames;Uid=root;Pwd=password;";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString),
        b => b.MigrationsAssembly("videogame-randomized-back")
    ));

// Add services to the container
builder.Services.AddScoped<SavedGamesService>();
builder.Services.AddScoped<GameMapper>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateGameDto>();

// Configure JSON options for Minimal APIs
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Apply pending migrations automatically on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseCors();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapSavedGamesEndpoints();
app.MapGet("/", () => "Hello World!");

app.Run();
