using Finova.Application.Interfaces;
using Finova.Domain.Entities;
using Finova.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Finova.Infrastructure.Repositories;

public class SavingGoalRepository : ISavingGoalRepository
{
    private readonly FinovaDbContext _context;

    public SavingGoalRepository(FinovaDbContext context)
    {
        _context = context;
    }

    public Task<List<SavingGoal>> ListByUserAsync(Guid userId) =>
        _context.SavingGoals
            .Where(g => g.UserId == userId)
            .OrderBy(g => g.TargetDate)
            .ToListAsync();

    public Task<SavingGoal?> GetOwnedAsync(Guid userId, Guid id) =>
        _context.SavingGoals.FirstOrDefaultAsync(g => g.Id == id && g.UserId == userId);

    public async Task AddAsync(SavingGoal goal)
    {
        _context.SavingGoals.Add(goal);
        await _context.SaveChangesAsync();
    }

    public void Remove(SavingGoal goal) => _context.SavingGoals.Remove(goal);

    public Task SaveChangesAsync() => _context.SaveChangesAsync();
}
