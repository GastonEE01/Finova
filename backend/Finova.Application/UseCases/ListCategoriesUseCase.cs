using Finova.Application.Interfaces;
using Finova.Domain.Enums;

namespace Finova.Application.UseCases;

public class CategoryResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public MovementType Type { get; set; }
}

public class ListCategoriesUseCase
{
    private readonly ICategoryRepository _categories;

    public ListCategoriesUseCase(ICategoryRepository categories)
    {
        _categories = categories;
    }

    public async Task<List<CategoryResponse>> ExecuteAsync(Guid userId, MovementType? type)
    {
        var categories = await _categories.ListAccessibleAsync(userId, type);
        return categories.Select(c => new CategoryResponse
        {
            Id = c.Id,
            Name = c.Name,
            Type = c.Type
        }).ToList();
    }
}
