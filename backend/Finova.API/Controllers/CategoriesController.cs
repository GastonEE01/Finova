using Finova.Domain.Enums;
using Finova.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Finova.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly Finova.Infrastructure.Persistence.FinovaDbContext _context;

    public CategoriesController(FinovaDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] MovementType? type)
    {
        var sub = User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(sub, out var userId))
            return Unauthorized();

        var query = _context.Categories.Where(c => c.UserId == userId || c.UserId == null);
        if (type.HasValue)
            query = query.Where(c => c.Type == type.Value);

        var categories = await query.OrderBy(c => c.Name)
            .Select(c => new { c.Id, c.Name, c.Type })
            .ToListAsync();

        return Ok(categories);
    }
}
