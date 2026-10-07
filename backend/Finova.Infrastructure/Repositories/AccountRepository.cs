using Finova.Application.Interfaces;
using Finova.Domain.Entities;
using Finova.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Finova.Infrastructure.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly FinovaDbContext _context;

    public AccountRepository(FinovaDbContext context)
    {
        _context = context;
    }

    public Task<List<Account>> ListByUserWithMovementsAsync(Guid userId) =>
        _context.Accounts
            .Where(a => a.UserId == userId)
            .Include(a => a.Movements)
            .ToListAsync();

    public Task<Account?> GetByUserWithMovementsAsync(Guid userId, Guid accountId) =>
        _context.Accounts
            .Where(a => a.UserId == userId && a.Id == accountId)
            .Include(a => a.Movements)
            .FirstOrDefaultAsync();

    public Task<Account?> GetOwnedAsync(Guid userId, Guid accountId) =>
        _context.Accounts.FirstOrDefaultAsync(a => a.Id == accountId && a.UserId == userId);

    public async Task AddAsync(Account account)
    {
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();
    }
}
