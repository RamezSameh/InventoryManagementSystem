using InventorySystem.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventorySystem.API.Controllers;

/// <summary>Admin-only user management: list users, change a role, disable/enable an account.</summary>
[ApiController]
[Route("api/users")]
[Authorize(Roles = "Admin")]
public class UsersController(UserManager<AppUser> users) : ControllerBase
{
    public record UserDto(string Id, string Email, string FullName, string[] Roles, bool IsLockedOut, DateTime CreatedAt);
    public record ChangeRoleRequest(string Role);
    public record SetLockoutRequest(bool LockedOut);

    private static readonly string[] ValidRoles = ["Admin", "Manager", "Cashier"];

    [HttpGet]
    public async Task<IActionResult> List()
    {
        var all = await users.Users.OrderBy(x => x.Email).ToListAsync();
        var result = new List<UserDto>();
        foreach (var u in all)
        {
            var roles = await users.GetRolesAsync(u);
            result.Add(new UserDto(
                u.Id,
                u.Email ?? string.Empty,
                u.FullName,
                roles.ToArray(),
                u.LockoutEnabled && u.LockoutEnd.HasValue && u.LockoutEnd.Value > DateTimeOffset.UtcNow,
                u.CreatedAt));
        }
        return Ok(result);
    }

    [HttpPut("{id}/role")]
    public async Task<IActionResult> ChangeRole(string id, ChangeRoleRequest r)
    {
        if (!ValidRoles.Any(v => v.Equals(r.Role, StringComparison.OrdinalIgnoreCase)))
            return BadRequest(new { error = "Invalid role." });
        if (id == users.GetUserId(User))
            return BadRequest(new { error = "You cannot change your own role." });
        var u = await users.FindByIdAsync(id);
        if (u is null)
            return NotFound(new { error = "User not found." });
        var current = await users.GetRolesAsync(u);
        var removed = await users.RemoveFromRolesAsync(u, current);
        if (!removed.Succeeded)
            return BadRequest(removed.Errors);
        var added = await users.AddToRoleAsync(u, r.Role);
        if (!added.Succeeded)
            return BadRequest(added.Errors);
        return NoContent();
    }

    [HttpPut("{id}/lockout")]
    public async Task<IActionResult> SetLockout(string id, SetLockoutRequest r)
    {
        if (id == users.GetUserId(User))
            return BadRequest(new { error = "You cannot lock out your own account." });
        var u = await users.FindByIdAsync(id);
        if (u is null)
            return NotFound(new { error = "User not found." });
        // Identity only enforces a lockout when LockoutEnabled is true, so enable
        // it together with the lockout end; unlocking clears the end date.
        await users.SetLockoutEnabledAsync(u, true);
        var result = await users.SetLockoutEndDateAsync(u,
            r.LockedOut ? DateTimeOffset.UtcNow.AddYears(100) : null);
        if (!result.Succeeded)
            return BadRequest(result.Errors);
        return NoContent();
    }
}
