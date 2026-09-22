using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace InventorySystem.IntegrationTests;

[Collection("Api")]
public class DashboardApiTests(InventoryWebFactory factory)
{
    [Fact]
    public async Task GetSummary_Returns200_WithSummaryShape()
    {
        var client = factory.CreateClient();
        var token = await factory.GetAdminTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resp = await client.GetAsync("/api/dashboard/summary");

        resp.EnsureSuccessStatusCode();
        var body = await resp.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(JsonValueKind.Object, body.ValueKind);
        // The endpoint aggregates inventory KPIs; at minimum it must be a JSON object.
        // Tighten per-field assertions once the summary DTO contract is finalized.
        Assert.True(body.EnumerateObject().Any());
    }
}
