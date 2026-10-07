using Finova.Domain.Entities;

namespace Finova.Application.Interfaces;

public interface ISavingGoalRepository
{
    Task<List<SavingGoal>> ListByUserAsync(Guid userId);
    Task<SavingGoal?> GetOwnedAsync(Guid userId, Guid id);
    Task AddAsync(SavingGoal goal);
    void Remove(SavingGoal goal);
    Task SaveChangesAsync();
}
