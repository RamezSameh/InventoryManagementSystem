using System.Security.Claims; using InventorySystem.Application.Interfaces; using Microsoft.AspNetCore.Http;
namespace InventorySystem.Infrastructure.Services;
public class CurrentUserService(IHttpContextAccessor accessor) : ICurrentUserService { private ClaimsPrincipal User=>accessor.HttpContext?.User??new ClaimsPrincipal(); public string? UserId=>User.FindFirstValue(ClaimTypes.NameIdentifier); public string? UserName=>User.Identity?.Name; public bool IsAuthenticated=>User.Identity?.IsAuthenticated??false; }
