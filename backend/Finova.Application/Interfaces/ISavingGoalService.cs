using Finova.Application.DTOs;

namespace Finova.Application.Interfaces;

public interface ISavingGoalService
{
    Task<List<SavingGoalResponse>> GetAllAsync(Guid userId);
    Task<SavingGoalResponse> CreateAsync(Guid userId, CreateSavingGoalRequest request);
    Task<SavingGoalResponse> UpdateAsync(Guid userId, Guid id, UpdateSavingGoalRequest request);
    Task DeleteAsync(Guid userId, Guid id);
    Task<ContributionResponse> AddContributionAsync(Guid userId, Guid goalId, AddContributionRequest request);
}
