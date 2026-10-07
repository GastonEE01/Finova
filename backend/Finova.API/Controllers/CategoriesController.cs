using Finova.Application.UseCases;
using Finova.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Finova.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly ListCategoriesUseCase _list;

    public CategoriesController(ListCategoriesUseCase list)
    {
        _list = list;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] MovementType? type)
    {
        var sub = User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(sub, out var userId))
            return Unauthorized();

        var categories = await _list.ExecuteAsync(userId, type);
        return Ok(categories);
    }
}
