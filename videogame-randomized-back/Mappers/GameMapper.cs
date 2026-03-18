using Riok.Mapperly.Abstractions;
using videogame_randomized_back.Models;
using videogame_randomized_back.DTOs;

namespace videogame_randomized_back.Mappers;

[Mapper]
public partial class GameMapper
{
    // Game -> GameDto
    public partial GameDto GameToDto(Game game);

    // List<Game> -> List<GameDto>
    public partial List<GameDto> GamesToDtos(List<Game> games);

    // CreateGameDto -> Game
    public partial Game CreateDtoToGame(CreateGameDto dto);

    // Genre mapping
    public partial GenreDto GenreToDto(Genre genre);
    public partial Genre DtoToGenre(GenreDto dto);
    
    // Platform mapping
    public partial PlatformDto PlatformToDto(Platform platform);
    public partial Platform DtoToPlatform(PlatformDto dto);

    // StatisticsDto -> GameStatsDto
    public partial GameStatsDto StatsToDto(StatisticsDto stats);
}
