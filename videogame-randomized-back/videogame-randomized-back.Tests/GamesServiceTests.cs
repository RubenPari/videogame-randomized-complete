using Microsoft.EntityFrameworkCore;
using videogame_randomized_back.Data;
using videogame_randomized_back.Models;
using videogame_randomized_back.Services;

namespace videogame_randomized_back.Tests;

public class GamesServiceTests
{
    private AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task RemoveAsync_ShouldReturnFalse_WhenGameDoesNotExist()
    {
        // Arrange
        await using var db = CreateDbContext();
        var service = new GamesService(db);
        const string userId = "user-1";
        const int nonExistentId = 999;

        // Act
        var result = await service.RemoveAsync(userId, nonExistentId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task RemoveAsync_ShouldReturnFalse_WhenGameBelongsToDifferentUser()
    {
        // Arrange
        await using var db = CreateDbContext();
        var service = new GamesService(db);
        var game = new Game
        {
            Id = 1,
            Name = "Test Game",
            Rating = 4.5,
            UserId = "other-user",
            SavedAt = DateTime.UtcNow,
            Genres = new List<Genre>(),
            Platforms = new List<Platform>()
        };
        db.Games.Add(game);
        await db.SaveChangesAsync();

        // Act
        var result = await service.RemoveAsync("different-user", 1);

        // Assert
        Assert.False(result);
        Assert.True(await db.Games.AnyAsync(g => g.Id == 1));
    }

    [Fact]
    public async Task RemoveAsync_ShouldReturnTrue_WhenGameBelongsToUser()
    {
        // Arrange
        await using var db = CreateDbContext();
        var service = new GamesService(db);
        var userId = "user-1";
        var game = new Game
        {
            Id = 1,
            Name = "Test Game",
            Rating = 4.5,
            UserId = userId,
            SavedAt = DateTime.UtcNow,
            Genres = new List<Genre>(),
            Platforms = new List<Platform>()
        };
        db.Games.Add(game);
        await db.SaveChangesAsync();

        // Act
        var result = await service.RemoveAsync(userId, 1);

        // Assert
        Assert.True(result);
        Assert.False(await db.Games.AnyAsync(g => g.Id == 1));
    }

    [Fact]
    public async Task GetByUserAsync_ShouldReturnOnlyUserGames()
    {
        // Arrange
        await using var db = CreateDbContext();
        var service = new GamesService(db);

        db.Games.Add(new Game
        {
            Id = 1,
            Name = "User1 Game",
            Rating = 4.0,
            UserId = "user-1",
            SavedAt = DateTime.UtcNow,
            Genres = new List<Genre>(),
            Platforms = new List<Platform>()
        });
        db.Games.Add(new Game
        {
            Id = 2,
            Name = "User2 Game",
            Rating = 3.5,
            UserId = "user-2",
            SavedAt = DateTime.UtcNow,
            Genres = new List<Genre>(),
            Platforms = new List<Platform>()
        });
        await db.SaveChangesAsync();

        // Act
        var result = await service.GetByUserAsync("user-1");

        // Assert
        Assert.Single(result);
        Assert.Equal("User1 Game", result[0].Name);
    }

    [Fact]
    public async Task ExistsByUserAsync_ShouldReturnTrue_WhenGameExists()
    {
        // Arrange
        await using var db = CreateDbContext();
        var service = new GamesService(db);

        db.Games.Add(new Game
        {
            Id = 1,
            Name = "Test Game",
            Rating = 4.0,
            UserId = "user-1",
            SavedAt = DateTime.UtcNow,
            Genres = new List<Genre>(),
            Platforms = new List<Platform>()
        });
        await db.SaveChangesAsync();

        // Act
        var result = await service.ExistsByUserAsync("user-1", 1);

        // Assert
        Assert.True(result);
    }
}
