using Finova.Application.DTOs;
using Finova.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Finova.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BudgetsController : ControllerBase
{
    private readonly IBudgetService _budgetService;

    public BudgetsController(IBudgetService budgetService)
    {
        _budgetService = budgetService;
    }

    [HttpGet]
    public async Task<IActionResult> GetByMonth([FromQuery] int? year, [FromQuery] int? month)
    {
        try
        {
            var now = DateTime.UtcNow;
            var budgets = await _budgetService.GetByMonthAsync(GetUserId(), year ?? now.Year, month ?? now.Month);
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
            var budget = await _budgetService.CreateAsync(GetUserId(), request);
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
            var budget = await _budgetService.UpdateAsync(GetUserId(), id, request);
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
            await _budgetService.DeleteAsync(GetUserId(), id);
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
