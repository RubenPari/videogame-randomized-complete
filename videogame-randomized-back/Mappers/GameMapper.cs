using Riok.Mapperly.Abstractions;
using videogame_randomized_back.Models;
using videogame_randomized_back.DTOs;
using videogame_randomized_back.Services; // For StatisticsDto

namespace videogame_randomized_back.Mappers;

[Mapper]
public partial class GameMapper
{
    // Game -> GameDto
    [MapProperty(nameof(Game.Platforms), nameof(GameDto.Platforms))]
    public partial GameDto GameToDto(Game game);

    // List<Game> -> List<GameDto>
    public partial List<GameDto> GamesToDtos(List<Game> games);

    // CreateGameDto -> Game
    [MapProperty(nameof(CreateGameDto.Platforms), nameof(Game.Platforms))]
    public partial Game CreateDtoToGame(CreateGameDto dto);

    // Manual mapping for PlatformWrapper list to PlatformDto list
    private List<PlatformDto>? MapPlatforms(List<PlatformWrapper>? wrappers)
    {
        return wrappers?.Select(w => new PlatformDto(w.Platform.Id, w.Platform.Name, w.Platform.Slug)).ToList();
    }

    // Manual mapping for PlatformDto list to PlatformWrapper list
    private List<PlatformWrapper>? MapPlatformDtos(List<PlatformDto>? dtos)
    {
        return dtos?.Select(d => new PlatformWrapper 
        { 
            Platform = new Platform { Id = d.Id, Name = d.Name, Slug = d.Slug } 
        }).ToList();
    }
    
    // Genre mapping
    public partial GenreDto GenreToDto(Genre genre);
    public partial Genre DtoToGenre(GenreDto dto);

    // StatisticsDto -> GameStatsDto
    public partial GameStatsDto StatsToDto(StatisticsDto stats);
}
