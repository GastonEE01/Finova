using Finova.Domain.Entities;

namespace Finova.Application.Interfaces;

public interface IBudgetRepository
{
    Task<List<Budget>> ListByMonthAsync(Guid userId, int year, int month);
    Task<bool> ExistsAsync(Guid userId, Guid categoryId, int year, int month, string currency);
    Task<Budget?> GetOwnedAsync(Guid userId, Guid id);
    Task AddAsync(Budget budget);
    void Remove(Budget budget);
    Task SaveChangesAsync();
}
