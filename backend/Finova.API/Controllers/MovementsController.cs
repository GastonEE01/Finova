using Finova.Application.DTOs;
using Finova.Application.UseCases;
using Finova.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Finova.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MovementsController : ControllerBase
{
    private readonly CreateMovementUseCase _create;
    private readonly ListMovementsUseCase _list;
    private readonly GetMovementHistoryUseCase _history;

    public MovementsController(
        CreateMovementUseCase create,
        ListMovementsUseCase list,
        GetMovementHistoryUseCase history)
    {
        _create = create;
        _list = list;
        _history = history;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var movements = await _list.ExecuteAsync(GetUserId());
        return Ok(movements);
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory([FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] Guid? accountId, [FromQuery] Guid? categoryId, [FromQuery] MovementType? type)
    {
        try
        {
            var history = await _history.ExecuteAsync(GetUserId(), from, to, accountId, categoryId, type);
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
            var movement = await _create.ExecuteAsync(GetUserId(), request);
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
