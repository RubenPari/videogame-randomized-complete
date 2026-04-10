using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace videogame_randomized_back.Tests;

/// <summary>Uses ASPNETCORE_ENVIRONMENT=Testing so startup skips DB migrations.</summary>
public sealed class TestingWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
    }
}
