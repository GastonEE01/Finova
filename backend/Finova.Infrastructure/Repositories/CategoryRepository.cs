using Finova.Application.Interfaces;
using Finova.Domain.Entities;
using Finova.Domain.Enums;
using Finova.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Finova.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly FinovaDbContext _context;

    public CategoryRepository(FinovaDbContext context)
    {
        _context = context;
    }

    public Task<Category?> GetAccessibleAsync(Guid userId, Guid categoryId) =>
        _context.Categories.FirstOrDefaultAsync(c => c.Id == categoryId && (c.UserId == userId || c.UserId == null));

    public Task<List<Category>> ListAccessibleAsync(Guid userId, MovementType? type)
    {
        var query = _context.Categories.Where(c => c.UserId == userId || c.UserId == null);
        if (type.HasValue)
            query = query.Where(c => c.Type == type.Value);
        return query.OrderBy(c => c.Name).ToListAsync();
    }
}
