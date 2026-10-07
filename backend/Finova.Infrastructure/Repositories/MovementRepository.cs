using Finova.Application.Interfaces;
using Finova.Domain.Entities;
using Finova.Domain.Enums;
using Finova.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Finova.Infrastructure.Repositories;

public class MovementRepository : IMovementRepository
{
    private readonly FinovaDbContext _context;

    public MovementRepository(FinovaDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Movement movement)
    {
        _context.Movements.Add(movement);
        await _context.SaveChangesAsync();
    }

    public Task<List<Movement>> ListByUserWithDetailsAsync(Guid userId) =>
        _context.Movements
            .Where(m => m.Account.UserId == userId)
            .Include(m => m.Account)
            .Include(m => m.Category)
            .ToListAsync();

    public Task<decimal> SumExpensesAsync(Guid userId, Guid categoryId, DateTime start, DateTime end, string currency) =>
        _context.Movements
            .Where(m => m.Account.UserId == userId
                && m.CategoryId == categoryId
                && m.Type == MovementType.Expense
                && m.Date >= start && m.Date < end
                && m.Account.Currency == currency)
            .SumAsync(m => (decimal?)m.Amount)
            .ContinueWith(t => t.Result ?? 0);

    public Task<decimal> SumByGoalAsync(Guid goalId) =>
        _context.Movements
            .Where(m => m.GoalId == goalId)
            .SumAsync(m => (decimal?)m.Amount)
            .ContinueWith(t => t.Result ?? 0);
}
