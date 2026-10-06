using Finova.Application.DTOs;
using Finova.Application.Interfaces;
using Finova.Domain.Entities;
using Finova.Domain.Enums;
using Finova.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Finova.Infrastructure.Services;

public class BudgetService : IBudgetService
{
    private readonly FinovaDbContext _context;

    public BudgetService(FinovaDbContext context)
    {
        _context = context;
    }

    public async Task<List<BudgetResponse>> GetByMonthAsync(Guid userId, int year, int month)
    {
        var budgets = await _context.Budgets
            .Where(b => b.UserId == userId && b.Year == year && b.Month == month)
            .Include(b => b.Category)
            .ToListAsync();

        var result = new List<BudgetResponse>();
        foreach (var b in budgets)
            result.Add(await MapAsync(userId, b));
        return result;
    }

    public async Task<BudgetResponse> CreateAsync(Guid userId, CreateBudgetRequest request)
    {
        ValidateAmount(request.Amount);
        ValidateMonth(request.Year, request.Month);
        if (string.IsNullOrWhiteSpace(request.Currency))
            throw new ArgumentException("La moneda es obligatoria.");
        var currency = request.Currency.Trim().ToUpperInvariant();

        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == request.CategoryId && (c.UserId == userId || c.UserId == null));
        if (category is null)
            throw new ArgumentException("Categoría no encontrada.");
        if (category.Type != MovementType.Expense)
            throw new ArgumentException("El presupuesto solo aplica a categorías de gasto.");

        var exists = await _context.Budgets.AnyAsync(b =>
            b.UserId == userId && b.CategoryId == request.CategoryId &&
            b.Year == request.Year && b.Month == request.Month && b.Currency == currency);
        if (exists)
            throw new ArgumentException("Ya existe un presupuesto para esa categoría, mes y moneda.");

        var budget = new Budget
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CategoryId = request.CategoryId,
            Year = request.Year,
            Month = request.Month,
            Amount = request.Amount,
            Currency = currency
        };

        _context.Budgets.Add(budget);
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            throw new ArgumentException("Ya existe un presupuesto para esa categoría, mes y moneda.");
        }

        budget.Category = category;
        return await MapAsync(userId, budget);
    }

    public async Task<BudgetResponse> UpdateAsync(Guid userId, Guid id, UpdateBudgetRequest request)
    {
        ValidateAmount(request.Amount);
        var budget = await _context.Budgets
            .Include(b => b.Category)
            .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);
        if (budget is null)
            throw new KeyNotFoundException("Presupuesto no encontrado.");

        budget.Amount = request.Amount;
        await _context.SaveChangesAsync();
        return await MapAsync(userId, budget);
    }

    public async Task DeleteAsync(Guid userId, Guid id)
    {
        var budget = await _context.Budgets.FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);
        if (budget is null)
            throw new KeyNotFoundException("Presupuesto no encontrado.");
        _context.Budgets.Remove(budget);
        await _context.SaveChangesAsync();
    }

    private static void ValidateAmount(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("El monto debe ser mayor a 0.");
    }

    private static void ValidateMonth(int year, int month)
    {
        if (month < 1 || month > 12)
            throw new ArgumentException("El mes es inválido.");
        if (year < 2000 || year > 2100)
            throw new ArgumentException("El año es inválido.");
    }

    private async Task<BudgetResponse> MapAsync(Guid userId, Budget budget)
    {
        var start = new DateTime(budget.Year, budget.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var end = start.AddMonths(1);
        var spent = await _context.Movements
            .Where(m => m.Account.UserId == userId
                && m.CategoryId == budget.CategoryId
                && m.Type == MovementType.Expense
                && m.Date >= start && m.Date < end
                && m.Account.Currency == budget.Currency)
            .SumAsync(m => (decimal?)m.Amount) ?? 0;

        var percent = budget.Amount > 0 ? Math.Round(spent / budget.Amount * 100, 2) : 0;
        var status = percent >= 100 ? "Superado" : percent >= 80 ? "Acercandose" : "Ok";

        return new BudgetResponse
        {
            Id = budget.Id,
            CategoryId = budget.CategoryId,
            CategoryName = budget.Category?.Name ?? string.Empty,
            Year = budget.Year,
            Month = budget.Month,
            Amount = budget.Amount,
            Currency = budget.Currency,
            Spent = spent,
            Remaining = budget.Amount - spent,
            Percent = percent,
            Status = status
        };
    }
}
