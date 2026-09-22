using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace InventorySystem.IntegrationTests;

[Collection("Api")]
public class StockApiTests(InventoryWebFactory factory)
{
    private async Task<HttpClient> AdminClientAsync()
    {
        var client = factory.CreateClient();
        var token = await factory.GetAdminTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private static async Task<int> CreateCategoryAsync(HttpClient client)
    {
        var resp = await client.PostAsJsonAsync("/api/categories",
            new { name = "Cat-" + Guid.NewGuid().ToString("N"), description = "test" });
        resp.EnsureSuccessStatusCode();
        return (await resp.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetInt32();
    }

    private static async Task<int> CreateWarehouseAsync(HttpClient client)
    {
        var resp = await client.PostAsJsonAsync("/api/warehouses",
            new { name = "WH-" + Guid.NewGuid().ToString("N"), location = "test" });
        resp.EnsureSuccessStatusCode();
        return (await resp.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetInt32();
    }

    private static async Task<(int ProductId, string Sku)> CreateProductAsync(HttpClient client, int categoryId, int minimumStock)
    {
        var sku = "SKU-" + Guid.NewGuid().ToString("N");
        var resp = await client.PostAsJsonAsync("/api/products", new
        {
            name = "Widget",
            sku,
            purchasePrice = 10m,
            sellingPrice = 15m,
            minimumStock,
            categoryId
        });
        resp.EnsureSuccessStatusCode();
        var id = (await resp.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetInt32();
        return (id, sku);
    }

    [Fact]
    public async Task PostMovement_Anonymous_Returns401()
    {
        var client = factory.CreateClient();

        var resp = await client.PostAsJsonAsync("/api/stock/movement",
            new { productId = 1, warehouseId = 1, quantity = 5, type = 1 });

        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task PostMovement_AsAdmin_FullFlow_Succeeds()
    {
        var client = await AdminClientAsync();
        var categoryId = await CreateCategoryAsync(client);
        var warehouseId = await CreateWarehouseAsync(client);
        var (productId, _) = await CreateProductAsync(client, categoryId, minimumStock: 0);

        var resp = await client.PostAsJsonAsync("/api/stock/movement",
            new { productId, warehouseId, quantity = 10, type = 1 }); // PurchaseIn

        resp.EnsureSuccessStatusCode();
        var id = (await resp.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetInt32();
        Assert.True(id > 0);
    }

    [Fact]
    public async Task PostMovement_ZeroQuantity_Returns400()
    {
        var client = await AdminClientAsync();
        var categoryId = await CreateCategoryAsync(client);
        var warehouseId = await CreateWarehouseAsync(client);
        var (productId, _) = await CreateProductAsync(client, categoryId, minimumStock: 0);

        var resp = await client.PostAsJsonAsync("/api/stock/movement",
            new { productId, warehouseId, quantity = 0, type = 1 });

        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    [Fact]
    public async Task PostMovement_TransferOut_WithoutDestination_Returns400()
    {
        var client = await AdminClientAsync();
        var categoryId = await CreateCategoryAsync(client);
        var warehouseId = await CreateWarehouseAsync(client);
        var (productId, _) = await CreateProductAsync(client, categoryId, minimumStock: 0);
        await client.PostAsJsonAsync("/api/stock/movement",
            new { productId, warehouseId, quantity = 10, type = 1 });

        var resp = await client.PostAsJsonAsync("/api/stock/movement",
            new { productId, warehouseId, quantity = 5, type = 4 }); // TransferOut, no destination

        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    [Fact]
    public async Task LowStock_Authenticated_Returns200_AndContainsLowItem()
    {
        var client = await AdminClientAsync();
        var categoryId = await CreateCategoryAsync(client);
        var warehouseId = await CreateWarehouseAsync(client);
        var (productId, sku) = await CreateProductAsync(client, categoryId, minimumStock: 100);
        await client.PostAsJsonAsync("/api/stock/movement",
            new { productId, warehouseId, quantity = 10, type = 1 });

        var resp = await client.GetAsync("/api/stock/low-stock");

        resp.EnsureSuccessStatusCode();
        var items = await resp.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(JsonValueKind.Array, items.ValueKind);
        Assert.Contains(items.EnumerateArray(),
            e => e.GetProperty("sku").GetString() == sku && e.GetProperty("quantity").GetInt32() == 10);
    }

    [Fact]
    public async Task GetMovements_AsAdmin_ReturnsPagedHistory()
    {
        var client = await AdminClientAsync();
        var categoryId = await CreateCategoryAsync(client);
        var warehouseId = await CreateWarehouseAsync(client);
        var (productId, _) = await CreateProductAsync(client, categoryId, minimumStock: 0);
        await client.PostAsJsonAsync("/api/stock/movement",
            new { productId, warehouseId, quantity = 10, type = 1 });

        var resp = await client.GetAsync($"/api/stock/movements?productId={productId}");

        resp.EnsureSuccessStatusCode();
        var body = await resp.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(body.GetProperty("total").GetInt32() >= 1);
        Assert.Equal(JsonValueKind.Array, body.GetProperty("items").ValueKind);
    }

    [Fact]
    public async Task GetMovements_Anonymous_Returns401()
    {
        var client = factory.CreateClient();

        var resp = await client.GetAsync("/api/stock/movements");

        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }
}
