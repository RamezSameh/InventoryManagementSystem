using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace InventorySystem.IntegrationTests;

[Collection("Api")]
public class AuthTests(InventoryWebFactory factory)
{
    // Registration is admin-only: register through the test admin's token.
    private async Task<HttpResponseMessage> RegisterAsAdminAsync(HttpClient client, string email)
    {
        var adminToken = await factory.GetAdminTokenAsync(client);
        var req = new HttpRequestMessage(HttpMethod.Post, "/api/auth/register");
        req.Content = JsonContent.Create(new { email, password = "Test@1234", fullName = "Test User" });
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        return await client.SendAsync(req);
    }

    [Fact]
    public async Task Register_AsAdmin_ReturnsToken()
    {
        var client = factory.CreateClient();
        var email = $"user-{Guid.NewGuid():N}@test.local";

        var resp = await RegisterAsAdminAsync(client, email);

        resp.EnsureSuccessStatusCode();
        var token = (await resp.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("token").GetString();
        Assert.False(string.IsNullOrWhiteSpace(token));
    }

    [Fact]
    public async Task Register_WithoutAuth_Returns401()
    {
        var client = factory.CreateClient();
        var email = $"user-{Guid.NewGuid():N}@test.local";

        var resp = await client.PostAsJsonAsync("/api/auth/register",
            new { email, password = "Test@1234", fullName = "Test User" });

        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task Login_WithRegisteredUser_ReturnsToken()
    {
        var client = factory.CreateClient();
        var email = $"user-{Guid.NewGuid():N}@test.local";
        (await RegisterAsAdminAsync(client, email)).EnsureSuccessStatusCode();

        var resp = await client.PostAsJsonAsync("/api/auth/login",
            new { email, password = "Test@1234" });

        resp.EnsureSuccessStatusCode();
        var token = (await resp.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("token").GetString();
        Assert.False(string.IsNullOrWhiteSpace(token));
    }

    [Fact]
    public async Task Login_WithWrongPassword_Returns401()
    {
        var client = factory.CreateClient();
        var email = $"user-{Guid.NewGuid():N}@test.local";
        (await RegisterAsAdminAsync(client, email)).EnsureSuccessStatusCode();

        var resp = await client.PostAsJsonAsync("/api/auth/login",
            new { email, password = "Wrong@9999" });

        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task Refresh_WithValidToken_ReturnsNewTokens_AndRotates()
    {
        var client = factory.CreateClient();
        var email = $"user-{Guid.NewGuid():N}@test.local";
        var reg = await RegisterAsAdminAsync(client, email);
        reg.EnsureSuccessStatusCode();
        var firstRefresh = (await reg.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("refreshToken").GetString();
        Assert.False(string.IsNullOrWhiteSpace(firstRefresh));

        var resp = await client.PostAsJsonAsync("/api/auth/refresh",
            new { refreshToken = firstRefresh });
        resp.EnsureSuccessStatusCode();
        var body = await resp.Content.ReadFromJsonAsync<JsonElement>();
        Assert.False(string.IsNullOrWhiteSpace(body.GetProperty("token").GetString()));
        var secondRefresh = body.GetProperty("refreshToken").GetString();
        Assert.False(string.IsNullOrWhiteSpace(secondRefresh));
        Assert.NotEqual(firstRefresh, secondRefresh);

        // The old refresh token was rotated on use — reuse must be rejected.
        var reuse = await client.PostAsJsonAsync("/api/auth/refresh",
            new { refreshToken = firstRefresh });
        Assert.Equal(HttpStatusCode.Unauthorized, reuse.StatusCode);
    }

    [Fact]
    public async Task Refresh_WithInvalidToken_Returns401()
    {
        var client = factory.CreateClient();

        var resp = await client.PostAsJsonAsync("/api/auth/refresh",
            new { refreshToken = "not-a-real-token" });

        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }
}
