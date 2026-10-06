using Finova.Application.DTOs;
using Finova.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Finova.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SavingGoalsController : ControllerBase
{
    private readonly ISavingGoalService _goalService;

    public SavingGoalsController(ISavingGoalService goalService)
    {
        _goalService = goalService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var goals = await _goalService.GetAllAsync(GetUserId());
        return Ok(goals);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateSavingGoalRequest request)
    {
        try
        {
            var goal = await _goalService.CreateAsync(GetUserId(), request);
            return CreatedAtAction(nameof(GetAll), new { id = goal.Id }, goal);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateSavingGoalRequest request)
    {
        try
        {
            var goal = await _goalService.UpdateAsync(GetUserId(), id, request);
            return Ok(goal);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { mensaje = "Meta no encontrada." });
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
            await _goalService.DeleteAsync(GetUserId(), id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { mensaje = "Meta no encontrada." });
        }
    }

    [HttpPost("{id:guid}/contributions")]
    public async Task<IActionResult> AddContribution(Guid id, AddContributionRequest request)
    {
        try
        {
            var goal = await _goalService.AddContributionAsync(GetUserId(), id, request);
            return Ok(goal);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensaje = ex.Message });
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
