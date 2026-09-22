using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace InventorySystem.IntegrationTests;

[Collection("Api")]
public class CatalogApiTests(InventoryWebFactory factory)
{
    [Fact]
    public async Task CreateCategory_AsAdmin_PersistsAndAppearsInList()
    {
        var client = factory.CreateClient();
        var token = await factory.GetAdminTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var name = "Cat-" + Guid.NewGuid().ToString("N");

        var create = await client.PostAsJsonAsync("/api/categories",
            new { name, description = "test category" });
        create.EnsureSuccessStatusCode();

        var list = await client.GetAsync("/api/categories");
        list.EnsureSuccessStatusCode();
        var items = await list.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Contains(items.EnumerateArray(), e => e.GetProperty("name").GetString() == name);
    }

    [Fact]
    public async Task CreateCategory_AsCashier_Returns403()
    {
        var client = factory.CreateClient();
        // Public registration always yields the Cashier role.
        var token = await InventoryWebFactory.RegisterAndGetTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resp = await client.PostAsJsonAsync("/api/categories",
            new { name = "Nope-" + Guid.NewGuid().ToString("N") });

        Assert.Equal(HttpStatusCode.Forbidden, resp.StatusCode);
    }

    [Fact]
    public async Task GetCategories_Anonymous_Returns200()
    {
        var client = factory.CreateClient();

        var resp = await client.GetAsync("/api/categories");

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }
}
