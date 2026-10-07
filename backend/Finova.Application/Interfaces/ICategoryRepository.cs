using Finova.Domain.Entities;
using Finova.Domain.Enums;

namespace Finova.Application.Interfaces;

public interface ICategoryRepository
{
    Task<Category?> GetAccessibleAsync(Guid userId, Guid categoryId);
    Task<List<Category>> ListAccessibleAsync(Guid userId, MovementType? type);
}
