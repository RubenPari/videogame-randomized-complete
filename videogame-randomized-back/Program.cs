using FluentValidation;
using System.Text.Json.Serialization;
using videogame_randomized_back.DTOs; // Added missing using
using videogame_randomized_back.Endpoints;
using videogame_randomized_back.Mappers;
using videogame_randomized_back.Services;
using videogame_randomized_back.Validators;
using videogame_randomized_back.Models;
using Microsoft.Extensions.Configuration; // Added for IConfiguration if needed explicitly

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Ensure configuration is available to the service
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

builder.Services.AddSingleton<SavedGamesService>();
builder.Services.AddSingleton<GameMapper>();
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

app.UseCors();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapSavedGamesEndpoints();
app.MapGet("/", () => "Hello World!");

app.Run();
