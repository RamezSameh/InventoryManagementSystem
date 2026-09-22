using System.Net.Http.Json;
using System.Text.Json;
using InventorySystem.Domain.Entities;
using InventorySystem.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InventorySystem.IntegrationTests;

/// <summary>
/// Boots the real API with an EF Core InMemory database and test JWT settings.
/// All tests share one factory (and therefore one database) via <see cref="ApiCollection"/>.
/// </summary>
public class InventoryWebFactory : WebApplicationFactory<Program>
{
    private const string AdminEmail = "admin-tests@inventory.local";
    private const string AdminPassword = "Admin@123!";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "integration-tests-only-not-a-real-secret-0123456789abcdef",
                ["Jwt:Issuer"] = "InventorySystem",
                ["Jwt:Audience"] = "InventorySystem.Client"
            });
        });

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor is not null)
                services.Remove(descriptor);

            services.AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase("IntegrationTestsDb"));
        });
    }

    /// <summary>
    /// Ensures the Admin/Manager/Cashier roles exist and returns a JWT for an
    /// admin user created directly through UserManager (test-only hook, no
    /// production code involved).
    /// </summary>
    public async Task<string> GetAdminTokenAsync(HttpClient client)
    {
        using var scope = Services.CreateScope();
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in new[] { "Admin", "Manager", "Cashier" })
            if (!await roles.RoleExistsAsync(role))
                await roles.CreateAsync(new IdentityRole(role));

        var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var admin = await users.FindByEmailAsync(AdminEmail);
        if (admin is null)
        {
            admin = new AppUser
            {
                UserName = AdminEmail,
                Email = AdminEmail,
                FullName = "Integration Tests Admin",
                EmailConfirmed = true
            };
            var created = await users.CreateAsync(admin, AdminPassword);
            if (!created.Succeeded)
                throw new InvalidOperationException("Could not create test admin: " +
                    string.Join("; ", created.Errors.Select(e => e.Description)));
            await users.AddToRoleAsync(admin, "Admin");
        }

        var resp = await client.PostAsJsonAsync("/api/auth/login",
            new { email = AdminEmail, password = AdminPassword });
        resp.EnsureSuccessStatusCode();
        var token = (await resp.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("token").GetString();
        return token ?? throw new InvalidOperationException("Login did not return a token.");
    }

    /// <summary>Registers a user through the public endpoint and returns its JWT.</summary>
    public static async Task<string> RegisterAndGetTokenAsync(HttpClient client, string? email = null)
    {
        email ??= $"user-{Guid.NewGuid():N}@test.local";
        var resp = await client.PostAsJsonAsync("/api/auth/register",
            new { email, password = "Test@1234", fullName = "Test User" });
        resp.EnsureSuccessStatusCode();
        var token = (await resp.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("token").GetString();
        return token ?? throw new InvalidOperationException("Register did not return a token.");
    }
}

[CollectionDefinition("Api")]
public class ApiCollection : ICollectionFixture<InventoryWebFactory>
{
}
