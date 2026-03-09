using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace videogame_randomized_back.Data;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        
        // Use a dummy connection string for design-time migrations
        // This won't actually connect to database during migration creation
        optionsBuilder.UseMySql(
            "Server=localhost;Database=videogames;Uid=root;Pwd=password;",
            new MySqlServerVersion(new Version(8, 0, 0))
        );

        return new AppDbContext(optionsBuilder.Options);
    }
}
