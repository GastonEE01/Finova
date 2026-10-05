using Finova.Application.DTOs;
using Finova.Application.Interfaces;
using Finova.Domain.Entities;
using Finova.Domain.Enums;
using Finova.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Finova.Infrastructure.Services;

public class AccountService : IAccountService
{
    private readonly FinovaDbContext _context;

    public AccountService(FinovaDbContext context)
    {
        _context = context;
    }

    public async Task<AccountResponse> CreateAsync(Guid userId, CreateAccountRequest request)
    {
        var account = new Account
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = request.Name.Trim(),
            Currency = request.Currency.Trim().ToUpperInvariant()
        };

        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        return new AccountResponse
        {
            Id = account.Id,
            Name = account.Name,
            Currency = account.Currency,
            Balance = 0
        };
    }

    public async Task<List<AccountResponse>> GetAllAsync(Guid userId)
    {
        return await _context.Accounts
            .Where(a => a.UserId == userId)
            .Select(a => new AccountResponse
            {
                Id = a.Id,
                Name = a.Name,
                Currency = a.Currency,
                Balance = a.Movements.Sum(m => m.Type == MovementType.Income ? m.Amount : -m.Amount)
            })
            .ToListAsync();
    }

    public async Task<AccountResponse?> GetByIdAsync(Guid userId, Guid accountId)
    {
        return await _context.Accounts
            .Where(a => a.UserId == userId && a.Id == accountId)
            .Select(a => new AccountResponse
            {
                Id = a.Id,
                Name = a.Name,
                Currency = a.Currency,
                Balance = a.Movements.Sum(m => m.Type == MovementType.Income ? m.Amount : -m.Amount)
            })
            .FirstOrDefaultAsync();
    }
}
