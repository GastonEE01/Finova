using Finova.Application.DTOs;

namespace Finova.Application.Interfaces;

public interface IAccountService
{
    Task<AccountResponse> CreateAsync(Guid userId, CreateAccountRequest request);
    Task<List<AccountResponse>> GetAllAsync(Guid userId);
    Task<AccountResponse?> GetByIdAsync(Guid userId, Guid accountId);
}
