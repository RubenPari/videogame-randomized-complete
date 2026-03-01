using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using videogame_randomized_back.DTOs;
using videogame_randomized_back.Mappers;
using videogame_randomized_back.Models;
using videogame_randomized_back.Services;
using videogame_randomized_back.Filters;

namespace videogame_randomized_back.Endpoints;

public static class SavedGamesEndpoints
{
    public static void MapSavedGamesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/saved-games")
            .WithTags("Saved Games");

        group.MapGet("/", GetSavedGames);
        group.MapGet("/{id:int}", GetSavedGameById);

        group.MapPost("/", CreateSavedGame)
            .AddEndpointFilter<ValidationFilter<CreateGameDto>>();

        group.MapPut("/{id:int}", UpdateSavedGame)
            .AddEndpointFilter<ValidationFilter<UpdateGameDto>>();

        group.MapDelete("/{id:int}", DeleteSavedGame);
        group.MapDelete("/", DeleteAllSavedGames);
        group.MapGet("/check/{id:int}", CheckSavedGame);
        group.MapGet("/statistics", GetStatistics);
        group.MapGet("/search", SearchSavedGames);
        group.MapGet("/export", ExportSavedGames);
        group.MapPost("/import", ImportSavedGames);
        group.MapPost("/{id:int}/note", AddNote);
        group.MapPost("/{id:int}/rating", AddRating);
    }

    private static async Task<Ok<List<GameDto>>> GetSavedGames(
        SavedGamesService service,
        GameMapper mapper)
    {
        var games = await service.GetAsync();
        return TypedResults.Ok(mapper.GamesToDtos(games));
    }

    private static async Task<Results<Ok<GameDto>, NotFound>> GetSavedGameById(
        int id,
        SavedGamesService service,
        GameMapper mapper)
    {
        var game = await service.GetAsync(id);
        return game is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(mapper.GameToDto(game));
    }

    private static async Task<Results<Created<GameDto>, Conflict<object>>> CreateSavedGame(
        CreateGameDto dto,
        SavedGamesService service,
        GameMapper mapper)
    {
        if (await service.ExistsAsync(dto.Id))
        {
            return TypedResults.Conflict((object)new { message = "Game already exists" });
        }

        var game = mapper.CreateDtoToGame(dto);
        await service.CreateAsync(game);
        var gameDto = mapper.GameToDto(game);

        return TypedResults.Created($"/api/saved-games/{game.Id}", gameDto);
    }

    private static async Task<Results<Ok, NotFound>> UpdateSavedGame(
        int id,
        UpdateGameDto dto,
        SavedGamesService service)
    {
        var game = await service.GetAsync(id);
        if (game is null) return TypedResults.NotFound();

        if (dto.PersonalRating.HasValue) game.PersonalRating = dto.PersonalRating;
        if (!string.IsNullOrEmpty(dto.Note)) game.Note = dto.Note;

        await service.UpdateAsync(id, game);
        return TypedResults.Ok();
    }

    private static async Task<Results<Ok, NotFound>> DeleteSavedGame(
        int id,
        SavedGamesService service)
    {
        if (!await service.ExistsAsync(id)) return TypedResults.NotFound();
        await service.RemoveAsync(id);
        return TypedResults.Ok();
    }

    private static async Task<Ok> DeleteAllSavedGames(SavedGamesService service)
    {
        await service.RemoveAllAsync();
        return TypedResults.Ok();
    }

    private static async Task<Ok<object>> CheckSavedGame(int id, SavedGamesService service)
    {
        var isSaved = await service.ExistsAsync(id);
        return TypedResults.Ok((object)new { isSaved });
    }

    private static async Task<Ok<object>> GetStatistics(
        SavedGamesService service,
        GameMapper mapper)
    {
        var stats = await service.GetStatisticsAsync();
        return TypedResults.Ok((object)new { statistics = mapper.StatsToDto(stats) });
    }

    private static async Task<Ok<object>> SearchSavedGames(
        [FromQuery] string q,
        SavedGamesService service,
        GameMapper mapper)
    {
        if (string.IsNullOrWhiteSpace(q)) return TypedResults.Ok((object)new { games = new List<GameDto>() });
        var games = await service.SearchAsync(q);
        return TypedResults.Ok((object)new { games = mapper.GamesToDtos(games) });
    }

    private static async Task<FileContentHttpResult> ExportSavedGames(SavedGamesService service)
    {
        var games = await service.GetAsync();
        var json = System.Text.Json.JsonSerializer.Serialize(games);
        var bytes = System.Text.Encoding.UTF8.GetBytes(json);
        return TypedResults.File(bytes, "application/json", "saved_games_export.json");
    }

    private static async Task<Ok> ImportSavedGames(
        [FromBody] List<CreateGameDto> gameDtos,
        SavedGamesService service,
        GameMapper mapper)
    {
        if (gameDtos == null || !gameDtos.Any()) return TypedResults.Ok();

        foreach (var dto in gameDtos)
        {
            var exists = await service.ExistsAsync(dto.Id);
            var game = mapper.CreateDtoToGame(dto);

            // If importing, we might want to preserve the imported data's specific fields if they were in the DTO?
            // The DTOs in import might lack 'SavedAt', 'Note', etc if they are just CreateGameDto.
            // Assuming import matches export format, we might need a FullGameDto. 
            // For now, mapping CreateGameDto to Game is safe.

            if (!exists)
            {
                await service.CreateAsync(game);
            }
            else
            {
                await service.UpdateAsync(game.Id, game);
            }
        }
        return TypedResults.Ok();
    }

    private static async Task<Results<Ok, NotFound>> AddNote(
        int id,
        [FromBody] NoteRequest request,
        SavedGamesService service)
    {
        var game = await service.GetAsync(id);
        if (game is null) return TypedResults.NotFound();

        game.Note = request.Note;
        await service.UpdateAsync(id, game);
        return TypedResults.Ok();
    }

    private static async Task<Results<Ok, NotFound>> AddRating(
        int id,
        [FromBody] RatingRequest request,
        SavedGamesService service)
    {
        var game = await service.GetAsync(id);
        if (game is null) return TypedResults.NotFound();

        game.PersonalRating = request.PersonalRating;
        await service.UpdateAsync(id, game);
        return TypedResults.Ok();
    }
}
