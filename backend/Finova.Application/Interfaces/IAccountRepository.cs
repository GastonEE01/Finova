using Finova.Domain.Entities;

namespace Finova.Application.Interfaces;

public interface IAccountRepository
{
    Task<List<Account>> ListByUserWithMovementsAsync(Guid userId);
    Task<Account?> GetByUserWithMovementsAsync(Guid userId, Guid accountId);
    Task<Account?> GetOwnedAsync(Guid userId, Guid accountId);
    Task AddAsync(Account account);
}
