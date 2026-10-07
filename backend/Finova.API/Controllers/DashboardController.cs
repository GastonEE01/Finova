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
    public async Task<IActionResult> Get()
    {
        var dashboard = await _get.ExecuteAsync(GetUserId());
        return Ok(dashboard);
    }

    [HttpGet("expenses-by-category")]
    public async Task<IActionResult> GetExpensesByCategory([FromQuery] string currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
            return BadRequest("Debe indicar la moneda.");
        var result = await _byCategory.ExecuteAsync(GetUserId(), currency.Trim().ToUpper());
        return Ok(result);
    }

    [HttpGet("income-vs-expenses")]
    public async Task<IActionResult> GetIncomeVsExpenses([FromQuery] string currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
            return BadRequest("Debe indicar la moneda.");
        var result = await _incomeVsExpenses.ExecuteAsync(GetUserId(), currency.Trim().ToUpper());
        return Ok(result);
    }

    [HttpGet("balance-evolution")]
    public async Task<IActionResult> GetBalanceEvolution([FromQuery] string currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
            return BadRequest("Debe indicar la moneda.");
        var result = await _evolution.ExecuteAsync(GetUserId(), currency.Trim().ToUpper());
        return Ok(result);
    }

    [HttpGet("comparisons")]
    public async Task<IActionResult> GetComparisons([FromQuery] string currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
            return BadRequest("Debe indicar la moneda.");
        var result = await _comparisons.ExecuteAsync(GetUserId(), currency.Trim().ToUpper());
        return Ok(result);
    }

    private Guid GetUserId()
    {
        var sub = User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(sub!);
    }
}
