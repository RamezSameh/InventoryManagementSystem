using MediatR; using InventorySystem.Application.Features.Stock.Commands; using InventorySystem.Application.Features.Stock.Queries; using Microsoft.AspNetCore.Mvc;
namespace InventorySystem.API.Controllers;
[ApiController][Route("api/stock")] public class StockController(IMediator m):ControllerBase { [HttpPost("movement")] public async Task<IActionResult> Add(AddStockCommand c)=>Ok(new{id=await m.Send(c)}); [HttpGet("low-stock")] public async Task<IActionResult> Low()=>Ok(await m.Send(new GetLowStockQuery())); }
