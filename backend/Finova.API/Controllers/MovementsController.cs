using Finova.Application.DTOs;
using Finova.Application.Interfaces;
using Finova.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Finova.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MovementsController : ControllerBase
{
    private readonly IMovementService _movementService;

    public MovementsController(IMovementService movementService)
    {
        _movementService = movementService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var movements = await _movementService.GetAllByUserAsync(GetUserId());
        return Ok(movements);
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory([FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] Guid? accountId, [FromQuery] Guid? categoryId, [FromQuery] MovementType? type)
    {
        try
        {
            var history = await _movementService.GetHistoryAsync(GetUserId(), from, to, accountId, categoryId, type);
            return Ok(history);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateMovementRequest request)
    {
        try
        {
            var movement = await _movementService.CreateAsync(GetUserId(), request);
            return CreatedAtAction(nameof(GetAll), new { id = movement.Id }, movement);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { mensaje = "Cuenta no encontrada." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    private Guid GetUserId()
    {
        var sub = User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(sub!);
    }
}
