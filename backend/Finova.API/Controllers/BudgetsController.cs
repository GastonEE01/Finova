using Finova.Application.DTOs;
using Finova.Application.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Finova.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BudgetsController : ControllerBase
{
    private readonly GetBudgetsByMonthUseCase _list;
    private readonly CreateBudgetUseCase _create;
    private readonly UpdateBudgetUseCase _update;
    private readonly DeleteBudgetUseCase _delete;

    public BudgetsController(
        GetBudgetsByMonthUseCase list,
        CreateBudgetUseCase create,
        UpdateBudgetUseCase update,
        DeleteBudgetUseCase delete)
    {
        _list = list;
        _create = create;
        _update = update;
        _delete = delete;
    }

    [HttpGet]
    public async Task<IActionResult> GetByMonth([FromQuery] int? year, [FromQuery] int? month)
    {
        try
        {
            var now = DateTime.UtcNow;
            var budgets = await _list.ExecuteAsync(GetUserId(), year ?? now.Year, month ?? now.Month);
            return Ok(budgets);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateBudgetRequest request)
    {
        try
        {
            var budget = await _create.ExecuteAsync(GetUserId(), request);
            return CreatedAtAction(nameof(GetByMonth), new { year = budget.Year, month = budget.Month }, budget);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateBudgetRequest request)
    {
        try
        {
            var budget = await _update.ExecuteAsync(GetUserId(), id, request);
            return Ok(budget);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { mensaje = "Presupuesto no encontrado." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _delete.ExecuteAsync(GetUserId(), id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { mensaje = "Presupuesto no encontrado." });
        }
    }

    private Guid GetUserId()
    {
        var sub = User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(sub!);
    }
}
