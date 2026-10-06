using Finova.Application.DTOs;

namespace Finova.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardResponse> GetAsync(Guid userId);
    Task<ExpensesByCategoryResponse> GetExpensesByCategoryAsync(Guid userId, string currency);
    Task<IncomeVsExpensesResponse> GetIncomeVsExpensesAsync(Guid userId, string currency);
    Task<BalanceEvolutionResponse> GetBalanceEvolutionAsync(Guid userId, string currency);
    Task<ComparisonsResponse> GetComparisonsAsync(Guid userId, string currency);
}
