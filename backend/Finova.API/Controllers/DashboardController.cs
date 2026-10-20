using Finova.Application.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Finova.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly GetDashboardUseCase _get;
    private readonly GetExpensesByCategoryUseCase _byCategory;
    private readonly GetIncomeVsExpensesUseCase _incomeVsExpenses;
    private readonly GetBalanceEvolutionUseCase _evolution;
    private readonly GetComparisonsUseCase _comparisons;

    public DashboardController(
        GetDashboardUseCase get,
        GetExpensesByCategoryUseCase byCategory,
        GetIncomeVsExpensesUseCase incomeVsExpenses,
        GetBalanceEvolutionUseCase evolution,
        GetComparisonsUseCase comparisons)
    {
        _get = get;
        _byCategory = byCategory;
        _incomeVsExpenses = incomeVsExpenses;
        _evolution = evolution;
        _comparisons = comparisons;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int? year, [FromQuery] int? month)
    {
        if (!ValidarPeriodo(year, month, out var mensaje)) return BadRequest(mensaje);
        var dashboard = await _get.ExecuteAsync(GetUserId(), year, month);
        return Ok(dashboard);
    }

    [HttpGet("expenses-by-category")]
    public async Task<IActionResult> GetExpensesByCategory([FromQuery] string currency, [FromQuery] int? year, [FromQuery] int? month)
    {
        if (string.IsNullOrWhiteSpace(currency))
            return BadRequest("Debe indicar la moneda.");
        if (!ValidarPeriodo(year, month, out var mensaje)) return BadRequest(mensaje);
        var result = await _byCategory.ExecuteAsync(GetUserId(), currency.Trim().ToUpper(), year, month);
        return Ok(result);
    }

    [HttpGet("income-vs-expenses")]
    public async Task<IActionResult> GetIncomeVsExpenses([FromQuery] string currency, [FromQuery] int? year, [FromQuery] int? month)
    {
        if (string.IsNullOrWhiteSpace(currency))
            return BadRequest("Debe indicar la moneda.");
        if (!ValidarPeriodo(year, month, out var mensaje)) return BadRequest(mensaje);
        var result = await _incomeVsExpenses.ExecuteAsync(GetUserId(), currency.Trim().ToUpper(), year, month);
        return Ok(result);
    }

    [HttpGet("balance-evolution")]
    public async Task<IActionResult> GetBalanceEvolution([FromQuery] string currency, [FromQuery] int? year, [FromQuery] int? month)
    {
        if (string.IsNullOrWhiteSpace(currency))
            return BadRequest("Debe indicar la moneda.");
        if (!ValidarPeriodo(year, month, out var mensaje)) return BadRequest(mensaje);
        var result = await _evolution.ExecuteAsync(GetUserId(), currency.Trim().ToUpper(), year, month);
        return Ok(result);
    }

    [HttpGet("comparisons")]
    public async Task<IActionResult> GetComparisons([FromQuery] string currency, [FromQuery] int? year, [FromQuery] int? month)
    {
        if (string.IsNullOrWhiteSpace(currency))
            return BadRequest("Debe indicar la moneda.");
        if (!ValidarPeriodo(year, month, out var mensaje)) return BadRequest(mensaje);
        var result = await _comparisons.ExecuteAsync(GetUserId(), currency.Trim().ToUpper(), year, month);
        return Ok(result);
    }

    private static bool ValidarPeriodo(int? year, int? month, out string? mensaje)
    {
        mensaje = null;
        if (year is < 2000 or > 2200) { mensaje = "Año inválido."; return false; }
        if (month is < 1 or > 12) { mensaje = "Mes inválido (debe estar entre 1 y 12)."; return false; }
        return true;
    }

    private Guid GetUserId()
    {
        var sub = User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(sub!);
    }
}
