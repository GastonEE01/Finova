using Finova.Application.DTOs;

namespace Finova.Application.Interfaces;

public interface IBudgetService
{
    Task<List<BudgetResponse>> GetByMonthAsync(Guid userId, int year, int month);
    Task<BudgetResponse> CreateAsync(Guid userId, CreateBudgetRequest request);
    Task<BudgetResponse> UpdateAsync(Guid userId, Guid id, UpdateBudgetRequest request);
    Task DeleteAsync(Guid userId, Guid id);
}
