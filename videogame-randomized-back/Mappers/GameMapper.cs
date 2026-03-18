using Riok.Mapperly.Abstractions;
using videogame_randomized_back.Models;
using videogame_randomized_back.DTOs;

namespace videogame_randomized_back.Mappers;

[Mapper]
public partial class GameMapper
{
    [MapperIgnoreSource(nameof(Game.User))]
    public partial GameDto GameToDto(Game game);

    public partial List<GameDto> GamesToDtos(IEnumerable<Game> games);

    [MapperIgnoreTarget(nameof(Game.PersonalRating))]
    [MapperIgnoreTarget(nameof(Game.Note))]
    [MapperIgnoreTarget(nameof(Game.SavedAt))]
    [MapperIgnoreTarget(nameof(Game.UserId))]
    [MapperIgnoreTarget(nameof(Game.User))]
    public partial Game CreateDtoToGame(CreateGameDto dto);

    [MapperIgnoreTarget(nameof(Game.SavedAt))]
    [MapperIgnoreTarget(nameof(Game.UserId))]
    [MapperIgnoreTarget(nameof(Game.User))]
    public partial void UpdateGameDtoToGame(UpdateGameDto dto, Game game);

    [MapperIgnoreSource(nameof(Genre.Games))]
    private partial GenreDto GenreToDto(Genre genre);

    [MapperIgnoreSource(nameof(Platform.Games))]
    private partial PlatformDto PlatformToDto(Platform platform);

    [MapperIgnoreTarget(nameof(Genre.Games))]
    private partial Genre DtoToGenre(GenreDto dto);

    [MapperIgnoreTarget(nameof(Platform.Games))]
    private partial Platform DtoToPlatform(PlatformDto dto);
    
    public partial GameStatsDto StatsToDto(StatisticsDto stats);
}
