using Finova.Application.DTOs;
using Finova.Application.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Finova.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SavingGoalsController : ControllerBase
{
    private readonly ListSavingGoalsUseCase _list;
    private readonly CreateSavingGoalUseCase _create;
    private readonly UpdateSavingGoalUseCase _update;
    private readonly DeleteSavingGoalUseCase _delete;
    private readonly AddGoalContributionUseCase _contribute;

    public SavingGoalsController(
        ListSavingGoalsUseCase list,
        CreateSavingGoalUseCase create,
        UpdateSavingGoalUseCase update,
        DeleteSavingGoalUseCase delete,
        AddGoalContributionUseCase contribute)
    {
        _list = list;
        _create = create;
        _update = update;
        _delete = delete;
        _contribute = contribute;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var goals = await _list.ExecuteAsync(GetUserId());
        return Ok(goals);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateSavingGoalRequest request)
    {
        try
        {
            var goal = await _create.ExecuteAsync(GetUserId(), request);
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
            var goal = await _update.ExecuteAsync(GetUserId(), id, request);
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
            await _delete.ExecuteAsync(GetUserId(), id);
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
            var goal = await _contribute.ExecuteAsync(GetUserId(), id, request);
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
