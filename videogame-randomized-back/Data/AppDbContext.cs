using Microsoft.EntityFrameworkCore;
using videogame_randomized_back.Models;

namespace videogame_randomized_back.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Game> Games => Set<Game>();
    public DbSet<Genre> Genres => Set<Genre>();
    public DbSet<Platform> Platforms => Set<Platform>();
    public DbSet<GameGenre> GameGenres => Set<GameGenre>();
    public DbSet<GamePlatform> GamePlatforms => Set<GamePlatform>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Game>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.BackgroundImage).HasMaxLength(500);
            entity.Property(e => e.Rating).HasPrecision(3, 2);
            entity.Property(e => e.DescriptionRaw).HasColumnType("TEXT");
            entity.Property(e => e.Note).HasColumnType("TEXT");
            
            entity.HasMany(e => e.Genres)
                .WithMany(g => g.Games)
                .UsingEntity<GameGenre>(
                    j => j.HasOne(jg => jg.Genre).WithMany().HasForeignKey(jg => jg.GenreId),
                    j => j.HasOne(jg => jg.Game).WithMany().HasForeignKey(jg => jg.GameId),
                    j => j.HasKey(t => new { t.GameId, t.GenreId })
                );
                
            entity.HasMany(e => e.Platforms)
                .WithMany(p => p.Games)
                .UsingEntity<GamePlatform>(
                    j => j.HasOne(jp => jp.Platform).WithMany().HasForeignKey(jp => jp.PlatformId),
                    j => j.HasOne(jp => jp.Game).WithMany().HasForeignKey(jp => jp.GameId),
                    j => j.HasKey(t => new { t.GameId, t.PlatformId })
                );
                
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.SavedAt);
        });

        modelBuilder.Entity<Genre>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Slug).HasMaxLength(100);
            entity.HasIndex(e => e.Name);
        });

        modelBuilder.Entity<Platform>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Slug).HasMaxLength(100);
            entity.HasIndex(e => e.Name);
        });
    }
}
