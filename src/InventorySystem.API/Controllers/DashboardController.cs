using MediatR; using InventorySystem.Application.Features.Dashboard.Queries; using Microsoft.AspNetCore.Authorization; using Microsoft.AspNetCore.Mvc;
namespace InventorySystem.API.Controllers;
[ApiController][Route("api/dashboard")][Authorize] public class DashboardController(IMediator m):ControllerBase { [HttpGet("summary")] public async Task<IActionResult> Summary()=>Ok(await m.Send(new GetDashboardSummaryQuery())); }
