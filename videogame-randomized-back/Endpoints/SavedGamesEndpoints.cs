using System.Security.Claims;
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
            .WithTags("Saved Games")
            .RequireAuthorization();

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

    private static string GetUserId(ClaimsPrincipal user) =>
        user.FindFirstValue(ClaimTypes.NameIdentifier)!;

    private static async Task<Ok<List<GameDto>>> GetSavedGames(
        ClaimsPrincipal user,
        SavedGamesService service,
        GameMapper mapper)
    {
        var userId = GetUserId(user);
        var games = await service.GetByUserAsync(userId);
        return TypedResults.Ok(mapper.GamesToDtos(games));
    }

    private static async Task<Results<Ok<GameDto>, NotFound>> GetSavedGameById(
        int id,
        ClaimsPrincipal user,
        SavedGamesService service,
        GameMapper mapper)
    {
        var userId = GetUserId(user);
        var game = await service.GetByUserAsync(userId, id);
        return game is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(mapper.GameToDto(game));
    }

    private static async Task<Results<Created<GameDto>, Conflict<object>>> CreateSavedGame(
        CreateGameDto dto,
        ClaimsPrincipal user,
        SavedGamesService service,
        GameMapper mapper)
    {
        var userId = GetUserId(user);

        if (await service.ExistsByUserAsync(userId, dto.Id))
        {
            return TypedResults.Conflict((object)new { message = "Game already exists" });
        }

        var game = mapper.CreateDtoToGame(dto);
        game.UserId = userId;
        await service.CreateAsync(game);
        var gameDto = mapper.GameToDto(game);

        return TypedResults.Created($"/api/saved-games/{game.Id}", gameDto);
    }

    private static async Task<Results<Ok, NotFound>> UpdateSavedGame(
        int id,
        UpdateGameDto dto,
        ClaimsPrincipal user,
        SavedGamesService service)
    {
        var userId = GetUserId(user);
        var game = await service.GetByUserAsync(userId, id);
        if (game is null) return TypedResults.NotFound();

        if (dto.PersonalRating.HasValue) game.PersonalRating = dto.PersonalRating;
        if (!string.IsNullOrEmpty(dto.Note)) game.Note = dto.Note;

        await service.UpdateAsync(id, game);
        return TypedResults.Ok();
    }

    private static async Task<Results<Ok, NotFound>> DeleteSavedGame(
        int id,
        ClaimsPrincipal user,
        SavedGamesService service)
    {
        var userId = GetUserId(user);
        if (!await service.ExistsByUserAsync(userId, id)) return TypedResults.NotFound();
        await service.RemoveAsync(id);
        return TypedResults.Ok();
    }

    private static async Task<Ok> DeleteAllSavedGames(
        ClaimsPrincipal user,
        SavedGamesService service)
    {
        var userId = GetUserId(user);
        await service.RemoveAllByUserAsync(userId);
        return TypedResults.Ok();
    }

    private static async Task<Ok<object>> CheckSavedGame(
        int id,
        ClaimsPrincipal user,
        SavedGamesService service)
    {
        var userId = GetUserId(user);
        var isSaved = await service.ExistsByUserAsync(userId, id);
        return TypedResults.Ok((object)new { isSaved });
    }

    private static async Task<Ok<object>> GetStatistics(
        ClaimsPrincipal user,
        SavedGamesService service,
        GameMapper mapper)
    {
        var userId = GetUserId(user);
        var stats = await service.GetStatisticsByUserAsync(userId);
        return TypedResults.Ok((object)new { statistics = mapper.StatsToDto(stats) });
    }

    private static async Task<Ok<object>> SearchSavedGames(
        [FromQuery] string q,
        ClaimsPrincipal user,
        SavedGamesService service,
        GameMapper mapper)
    {
        var userId = GetUserId(user);
        if (string.IsNullOrWhiteSpace(q)) return TypedResults.Ok((object)new { games = new List<GameDto>() });
        var games = await service.SearchByUserAsync(userId, q);
        return TypedResults.Ok((object)new { games = mapper.GamesToDtos(games) });
    }

    private static async Task<FileContentHttpResult> ExportSavedGames(
        ClaimsPrincipal user,
        SavedGamesService service)
    {
        var userId = GetUserId(user);
        var games = await service.GetByUserAsync(userId);
        var json = System.Text.Json.JsonSerializer.Serialize(games);
        var bytes = System.Text.Encoding.UTF8.GetBytes(json);
        return TypedResults.File(bytes, "application/json", "saved_games_export.json");
    }

    private static async Task<Ok> ImportSavedGames(
        [FromBody] List<CreateGameDto>? gameDtos,
        ClaimsPrincipal user,
        SavedGamesService service,
        GameMapper mapper)
    {
        var userId = GetUserId(user);
        if (gameDtos == null || gameDtos.Count == 0) return TypedResults.Ok();

        foreach (var dto in gameDtos)
        {
            var exists = await service.ExistsByUserAsync(userId, dto.Id);
            var game = mapper.CreateDtoToGame(dto);
            game.UserId = userId;

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
        ClaimsPrincipal user,
        SavedGamesService service)
    {
        var userId = GetUserId(user);
        var game = await service.GetByUserAsync(userId, id);
        if (game is null) return TypedResults.NotFound();

        game.Note = request.Note;
        await service.UpdateAsync(id, game);
        return TypedResults.Ok();
    }

    private static async Task<Results<Ok, NotFound>> AddRating(
        int id,
        [FromBody] RatingRequest request,
        ClaimsPrincipal user,
        SavedGamesService service)
    {
        var userId = GetUserId(user);
        var game = await service.GetByUserAsync(userId, id);
        if (game is null) return TypedResults.NotFound();

        game.PersonalRating = request.PersonalRating;
        await service.UpdateAsync(id, game);
        return TypedResults.Ok();
    }
}
