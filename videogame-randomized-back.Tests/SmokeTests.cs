namespace videogame_randomized_back.Tests;

public class SmokeTests
{
    [Fact]
    public async Task Root_returns_hello_world()
    {
        await using var factory = new TestingWebApplicationFactory();
        var client = factory.CreateClient();
        var response = await client.GetAsync("/");
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync();
        Assert.Equal("Hello World!", body);
    }
}
