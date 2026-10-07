using Finova.Application.Interfaces;
using Finova.Domain.Entities;
using Finova.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Finova.Infrastructure.Repositories;

public class BudgetRepository : IBudgetRepository
{
    private readonly FinovaDbContext _context;

    public BudgetRepository(FinovaDbContext context)
    {
        _context = context;
    }

    public Task<List<Budget>> ListByMonthAsync(Guid userId, int year, int month) =>
        _context.Budgets
            .Where(b => b.UserId == userId && b.Year == year && b.Month == month)
            .Include(b => b.Category)
            .ToListAsync();

    public Task<bool> ExistsAsync(Guid userId, Guid categoryId, int year, int month, string currency) =>
        _context.Budgets.AnyAsync(b =>
            b.UserId == userId && b.CategoryId == categoryId &&
            b.Year == year && b.Month == month && b.Currency == currency);

    public Task<Budget?> GetOwnedAsync(Guid userId, Guid id) =>
        _context.Budgets
            .Include(b => b.Category)
            .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

    public async Task AddAsync(Budget budget)
    {
        _context.Budgets.Add(budget);
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException("Ya existe un presupuesto para esa categoría, mes y moneda.", ex);
        }
    }

    public void Remove(Budget budget) => _context.Budgets.Remove(budget);

    public Task SaveChangesAsync() => _context.SaveChangesAsync();
}
