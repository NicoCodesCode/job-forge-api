using Microsoft.AspNetCore.Mvc.Testing;

namespace JobForgeAPI.Tests;

[Collection("Database collection")]
public class HealthCheckTests
{
    private readonly TestWebApplicationFactory _factory;

    public HealthCheckTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task HealthEndpoint_ReturnsOk()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health");
        
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }
}