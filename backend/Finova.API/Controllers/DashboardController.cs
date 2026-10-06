using Finova.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Finova.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var dashboard = await _dashboardService.GetAsync(GetUserId());
        return Ok(dashboard);
    }

    [HttpGet("expenses-by-category")]
    public async Task<IActionResult> GetExpensesByCategory([FromQuery] string currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
            return BadRequest("Debe indicar la moneda.");
        var result = await _dashboardService.GetExpensesByCategoryAsync(GetUserId(), currency.Trim().ToUpper());
        return Ok(result);
    }

    [HttpGet("income-vs-expenses")]
    public async Task<IActionResult> GetIncomeVsExpenses([FromQuery] string currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
            return BadRequest("Debe indicar la moneda.");
        var result = await _dashboardService.GetIncomeVsExpensesAsync(GetUserId(), currency.Trim().ToUpper());
        return Ok(result);
    }

    [HttpGet("balance-evolution")]
    public async Task<IActionResult> GetBalanceEvolution([FromQuery] string currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
            return BadRequest("Debe indicar la moneda.");
        var result = await _dashboardService.GetBalanceEvolutionAsync(GetUserId(), currency.Trim().ToUpper());
        return Ok(result);
    }

    [HttpGet("comparisons")]
    public async Task<IActionResult> GetComparisons([FromQuery] string currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
            return BadRequest("Debe indicar la moneda.");
        var result = await _dashboardService.GetComparisonsAsync(GetUserId(), currency.Trim().ToUpper());
        return Ok(result);
    }

    private Guid GetUserId()
    {
        var sub = User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(sub!);
    }
}
