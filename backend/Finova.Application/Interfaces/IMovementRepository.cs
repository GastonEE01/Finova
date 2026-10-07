using Finova.Domain.Entities;

namespace Finova.Application.Interfaces;

public interface IMovementRepository
{
    Task AddAsync(Movement movement);
    Task<List<Movement>> ListByUserWithDetailsAsync(Guid userId);
    Task<decimal> SumExpensesAsync(Guid userId, Guid categoryId, DateTime start, DateTime end, string currency);
    Task<decimal> SumByGoalAsync(Guid goalId);
}
