using System.Net.Http.Json;
using Xunit;

namespace JobForgeAPI.Tests;

[Collection("Database collection")]
public class AuthControllerTests
{
    private readonly HttpClient _client;

    public AuthControllerTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_WithNewUsername_ReturnsCreated()
    {
        var username = $"user_{Guid.NewGuid():N}";

        var response = await _client.PostAsJsonAsync("/auth/register", new
        {
            Username = username,
            Password = "password123",
            OrganizationName = $"org_{Guid.NewGuid():N}"
        });

        Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithDuplicateUsername_ReturnsConflict()
    {
        var username = $"user_{Guid.NewGuid():N}";
        var payload = new { Username = username, Password = "password123", OrganizationName = "shared-org" };

        await _client.PostAsJsonAsync("/auth/register", payload);
        var secondResponse = await _client.PostAsJsonAsync("/auth/register", payload);

        Assert.Equal(System.Net.HttpStatusCode.Conflict, secondResponse.StatusCode);
    }

    [Fact]
    public async Task Login_WithCorrectCredentials_ReturnsToken()
    {
        var username = $"user_{Guid.NewGuid():N}";
        await _client.PostAsJsonAsync("/auth/register", new
        {
            Username = username,
            Password = "password123",
            OrganizationName = "login-test-org"
        });

        var response = await _client.PostAsJsonAsync("/auth/login", new { Username = username, Password = "password123" });

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        var token = await response.Content.ReadAsStringAsync();
        Assert.False(string.IsNullOrWhiteSpace(token));
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        var username = $"user_{Guid.NewGuid():N}";
        await _client.PostAsJsonAsync("/auth/register", new
        {
            Username = username,
            Password = "password123",
            OrganizationName = "login-fail-org"
        });

        var response = await _client.PostAsJsonAsync("/auth/login", new { Username = username, Password = "wrong" });

        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }
}