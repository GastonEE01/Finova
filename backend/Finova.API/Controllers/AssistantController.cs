using Finova.Application.DTOs;
using Finova.Application.Exceptions;
using Finova.Application.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Finova.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AssistantController : ControllerBase
{
    private readonly AskAssistantUseCase _ask;

    public AssistantController(AskAssistantUseCase ask)
    {
        _ask = ask;
    }

    [HttpPost("chat")]
    public async Task<IActionResult> Chat([FromBody] ChatRequest request)
    {
        var question = request?.Question?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(question))
            return BadRequest("Escribí una pregunta.");
        if (question.Length > 500)
            return BadRequest("La pregunta es demasiado larga (máximo 500 caracteres).");

        try
        {
            var answer = await _ask.ExecuteAsync(GetUserId(), question);
            return Ok(answer);
        }
        catch (AssistantUnavailableException)
        {
            return StatusCode(503, new { mensaje = "Asistente no disponible, intentá más tarde" });
        }
    }

    private Guid GetUserId()
    {
        var sub = User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(sub!);
    }
}
