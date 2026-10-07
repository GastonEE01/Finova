using Finova.Application.DTOs;
using Finova.Application.Interfaces;
using Finova.Domain.Entities;
using Finova.Domain.Enums;

namespace Finova.Application.UseCases;

public static class BudgetMapping
{
    public static async Task<BudgetResponse> ToResponseAsync(Budget budget, IMovementRepository movements)
    {
        var start = new DateTime(budget.Year, budget.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var end = start.AddMonths(1);
        var spent = await movements.SumExpensesAsync(
            budget.UserId, budget.CategoryId, start, end, budget.Currency);

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

    public static void ValidateAmount(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("El monto debe ser mayor a 0.");
    }

    public static void ValidateMonth(int year, int month)
    {
        if (month < 1 || month > 12)
            throw new ArgumentException("El mes es inválido.");
        if (year < 2000 || year > 2100)
            throw new ArgumentException("El año es inválido.");
    }
}

public class GetBudgetsByMonthUseCase
{
    private readonly IBudgetRepository _budgets;
    private readonly IMovementRepository _movements;

    public GetBudgetsByMonthUseCase(IBudgetRepository budgets, IMovementRepository movements)
    {
        _budgets = budgets;
        _movements = movements;
    }

    public async Task<List<BudgetResponse>> ExecuteAsync(Guid userId, int year, int month)
    {
        var budgets = await _budgets.ListByMonthAsync(userId, year, month);
        var result = new List<BudgetResponse>();
        foreach (var b in budgets)
            result.Add(await BudgetMapping.ToResponseAsync(b, _movements));
        return result;
    }
}

public class CreateBudgetUseCase
{
    private readonly IBudgetRepository _budgets;
    private readonly ICategoryRepository _categories;
    private readonly IMovementRepository _movements;

    public CreateBudgetUseCase(
        IBudgetRepository budgets,
        ICategoryRepository categories,
        IMovementRepository movements)
    {
        _budgets = budgets;
        _categories = categories;
        _movements = movements;
    }

    public async Task<BudgetResponse> ExecuteAsync(Guid userId, CreateBudgetRequest request)
    {
        BudgetMapping.ValidateAmount(request.Amount);
        BudgetMapping.ValidateMonth(request.Year, request.Month);
        if (string.IsNullOrWhiteSpace(request.Currency))
            throw new ArgumentException("La moneda es obligatoria.");
        var currency = request.Currency.Trim().ToUpperInvariant();

        var category = await _categories.GetAccessibleAsync(userId, request.CategoryId);
        if (category is null)
            throw new ArgumentException("Categoría no encontrada.");
        if (category.Type != MovementType.Expense)
            throw new ArgumentException("El presupuesto solo aplica a categorías de gasto.");

        if (await _budgets.ExistsAsync(userId, request.CategoryId, request.Year, request.Month, currency))
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

        try
        {
            await _budgets.AddAsync(budget);
        }
        catch (InvalidOperationException ex) when (ex.Message.StartsWith("Ya existe"))
        {
            throw new ArgumentException("Ya existe un presupuesto para esa categoría, mes y moneda.");
        }

        budget.Category = category;
        return await BudgetMapping.ToResponseAsync(budget, _movements);
    }
}

public class UpdateBudgetUseCase
{
    private readonly IBudgetRepository _budgets;
    private readonly IMovementRepository _movements;

    public UpdateBudgetUseCase(IBudgetRepository budgets, IMovementRepository movements)
    {
        _budgets = budgets;
        _movements = movements;
    }

    public async Task<BudgetResponse> ExecuteAsync(Guid userId, Guid id, UpdateBudgetRequest request)
    {
        BudgetMapping.ValidateAmount(request.Amount);
        var budget = await _budgets.GetOwnedAsync(userId, id);
        if (budget is null)
            throw new KeyNotFoundException("Presupuesto no encontrado.");

        budget.Amount = request.Amount;
        await _budgets.SaveChangesAsync();
        return await BudgetMapping.ToResponseAsync(budget, _movements);
    }
}

public class DeleteBudgetUseCase
{
    private readonly IBudgetRepository _budgets;

    public DeleteBudgetUseCase(IBudgetRepository budgets)
    {
        _budgets = budgets;
    }

    public async Task ExecuteAsync(Guid userId, Guid id)
    {
        var budget = await _budgets.GetOwnedAsync(userId, id);
        if (budget is null)
            throw new KeyNotFoundException("Presupuesto no encontrado.");
        _budgets.Remove(budget);
        await _budgets.SaveChangesAsync();
    }
}
