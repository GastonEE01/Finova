using Finova.Application.DTOs;
using Finova.Application.Interfaces;
using Finova.Domain.Entities;
using Finova.Domain.Enums;

namespace Finova.Application.UseCases;

public static class DashboardCalculations
{
    public static decimal SignedAmount(Movement m) =>
        m.Type == MovementType.Income ? m.Amount : -m.Amount;

    public static List<CurrencyTotal> GroupTotals(IEnumerable<(string Currency, decimal Amount)> items) =>
        items.GroupBy(x => x.Currency)
            .Select(g => new CurrencyTotal { Currency = g.Key, Amount = g.Sum(x => x.Amount) })
            .ToList();

    public static decimal? Variation(decimal current, decimal previous) =>
        previous == 0 ? null : Math.Round((current - previous) / previous * 100, 2);
}

public class GetDashboardUseCase
{
    private readonly IAccountRepository _accounts;
    private readonly IMovementRepository _movements;

    public GetDashboardUseCase(IAccountRepository accounts, IMovementRepository movements)
    {
        _accounts = accounts;
        _movements = movements;
    }

    public async Task<DashboardResponse> ExecuteAsync(Guid userId)
    {
        var now = DateTime.UtcNow;
        var from = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var accounts = await _accounts.ListByUserWithMovementsAsync(userId);

        // Saldo total por moneda: suma de saldos de cuentas (ingresos - gastos).
        var totalBalances = accounts
            .GroupBy(a => a.Currency)
            .Select(g => new CurrencyTotal
            {
                Currency = g.Key,
                Amount = g.Sum(a => a.Movements.Sum(DashboardCalculations.SignedAmount))
            })
            .ToList();

        var allMovements = await _movements.ListByUserWithDetailsAsync(userId);

        var monthMovements = allMovements
            .Where(m => m.Date.Year == now.Year && m.Date.Month == now.Month)
            .ToList();

        var totalIncome = monthMovements
            .Where(m => m.Type == MovementType.Income)
            .GroupBy(m => m.Account.Currency)
            .Select(g => new CurrencyTotal { Currency = g.Key, Amount = g.Sum(x => x.Amount) })
            .ToList();

        var totalExpenses = monthMovements
            .Where(m => m.Type == MovementType.Expense)
            .GroupBy(m => m.Account.Currency)
            .Select(g => new CurrencyTotal { Currency = g.Key, Amount = g.Sum(x => x.Amount) })
            .ToList();

        var recent = allMovements
            .OrderByDescending(m => m.Date)
            .ThenByDescending(m => m.Id)
            .Take(5)
            .Select(m => new MovementHistoryResponse
            {
                Id = m.Id,
                AccountId = m.AccountId,
                AccountName = m.Account.Name,
                Currency = m.Account.Currency,
                CategoryId = m.CategoryId,
                CategoryName = m.Category?.Name,
                Type = m.Type,
                Amount = m.Amount,
                Date = m.Date,
                Description = m.Description,
                RunningBalance = 0
            })
            .ToList();

        return new DashboardResponse
        {
            TotalBalances = totalBalances,
            TotalIncome = totalIncome,
            TotalExpenses = totalExpenses,
            RecentMovements = recent,
            PeriodFrom = from,
            PeriodTo = now
        };
    }
}

public class GetExpensesByCategoryUseCase
{
    private readonly IMovementRepository _movements;

    public GetExpensesByCategoryUseCase(IMovementRepository movements)
    {
        _movements = movements;
    }

    public async Task<ExpensesByCategoryResponse> ExecuteAsync(Guid userId, string currency)
    {
        var now = DateTime.UtcNow;
        var from = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var items = (await _movements.ListByUserWithDetailsAsync(userId))
            .Where(m => m.Account.Currency == currency
                && m.Type == MovementType.Expense
                && m.Date.Year == now.Year
                && m.Date.Month == now.Month)
            .GroupBy(m => new { m.CategoryId, CategoryName = m.Category != null ? m.Category.Name : null })
            .Select(g => new CategoryExpenseItem
            {
                CategoryId = g.Key.CategoryId,
                CategoryName = g.Key.CategoryName ?? "Sin categoría",
                Total = g.Sum(m => m.Amount)
            })
            .OrderByDescending(x => x.Total)
            .ToList();

        return new ExpensesByCategoryResponse
        {
            Currency = currency,
            PeriodFrom = from,
            PeriodTo = now,
            Items = items
        };
    }
}

public class GetIncomeVsExpensesUseCase
{
    private readonly IMovementRepository _movements;

    public GetIncomeVsExpensesUseCase(IMovementRepository movements)
    {
        _movements = movements;
    }

    public async Task<IncomeVsExpensesResponse> ExecuteAsync(Guid userId, string currency)
    {
        var now = DateTime.UtcNow;
        var start = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-5);

        var movements = (await _movements.ListByUserWithDetailsAsync(userId))
            .Where(m => m.Account.Currency == currency && m.Date >= start)
            .ToList();

        var months = new List<MonthlyTotalItem>();
        for (var i = 0; i < 6; i++)
        {
            var monthDate = start.AddMonths(i);
            var inMonth = movements.Where(m => m.Date.Year == monthDate.Year && m.Date.Month == monthDate.Month);
            months.Add(new MonthlyTotalItem
            {
                Year = monthDate.Year,
                Month = monthDate.Month,
                Income = inMonth.Where(m => m.Type == MovementType.Income).Sum(m => m.Amount),
                Expense = inMonth.Where(m => m.Type == MovementType.Expense).Sum(m => m.Amount)
            });
        }

        return new IncomeVsExpensesResponse { Currency = currency, Months = months };
    }
}

public class GetBalanceEvolutionUseCase
{
    private readonly IMovementRepository _movements;

    public GetBalanceEvolutionUseCase(IMovementRepository movements)
    {
        _movements = movements;
    }

    public async Task<BalanceEvolutionResponse> ExecuteAsync(Guid userId, string currency)
    {
        var now = DateTime.UtcNow;
        var from = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var movements = (await _movements.ListByUserWithDetailsAsync(userId))
            .Where(m => m.Account.Currency == currency && m.Date <= now)
            .ToList();

        var points = new List<BalancePointItem>();
        for (var day = 1; day <= now.Day; day++)
        {
            var cutoff = new DateTime(now.Year, now.Month, day, 23, 59, 59, DateTimeKind.Utc);
            var balance = movements
                .Where(m => m.Date <= cutoff)
                .Sum(DashboardCalculations.SignedAmount);
            points.Add(new BalancePointItem
            {
                Date = new DateTime(now.Year, now.Month, day, 0, 0, 0, DateTimeKind.Utc),
                Balance = balance
            });
        }

        return new BalanceEvolutionResponse
        {
            Currency = currency,
            PeriodFrom = from,
            PeriodTo = now,
            Points = points
        };
    }
}

public class GetComparisonsUseCase
{
    private readonly IMovementRepository _movements;

    public GetComparisonsUseCase(IMovementRepository movements)
    {
        _movements = movements;
    }

    public async Task<ComparisonsResponse> ExecuteAsync(Guid userId, string currency)
    {
        var now = DateTime.UtcNow;
        var curStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var prevStart = curStart.AddMonths(-1);
        var evoStart = curStart.AddMonths(-5);

        var movements = (await _movements.ListByUserWithDetailsAsync(userId))
            .Where(m => m.Account.Currency == currency && m.Date >= evoStart)
            .ToList();

        decimal SumByMonth(MovementType type, int year, int month) =>
            movements.Where(m => m.Type == type && m.Date.Year == year && m.Date.Month == month)
                .Sum(m => m.Amount);

        var current = new MonthTotalItem
        {
            Year = now.Year,
            Month = now.Month,
            Income = SumByMonth(MovementType.Income, now.Year, now.Month),
            Expense = SumByMonth(MovementType.Expense, now.Year, now.Month)
        };
        var previous = new MonthTotalItem
        {
            Year = prevStart.Year,
            Month = prevStart.Month,
            Income = SumByMonth(MovementType.Income, prevStart.Year, prevStart.Month),
            Expense = SumByMonth(MovementType.Expense, prevStart.Year, prevStart.Month)
        };

        var currentExpenses = movements
            .Where(m => m.Type == MovementType.Expense && m.Date.Year == now.Year && m.Date.Month == now.Month)
            .ToList();
        var monthTotal = currentExpenses.Sum(m => m.Amount);
        var top = currentExpenses
            .GroupBy(m => new { m.CategoryId, Name = m.Category?.Name ?? "Sin categoría" })
            .Select(g => new TopCategoryItem
            {
                CategoryId = g.Key.CategoryId,
                CategoryName = g.Key.Name,
                Total = g.Sum(x => x.Amount),
                Percent = monthTotal == 0 ? 0 : Math.Round(g.Sum(x => x.Amount) / monthTotal * 100, 2)
            })
            .OrderByDescending(x => x.Total)
            .Take(5)
            .ToList();

        var evolution = new List<ExpenseEvolutionItem>();
        for (var i = 0; i < 6; i++)
        {
            var monthDate = evoStart.AddMonths(i);
            evolution.Add(new ExpenseEvolutionItem
            {
                Year = monthDate.Year,
                Month = monthDate.Month,
                Expense = SumByMonth(MovementType.Expense, monthDate.Year, monthDate.Month)
            });
        }

        return new ComparisonsResponse
        {
            Currency = currency,
            CurrentMonth = current,
            PreviousMonth = previous,
            IncomeVariationPct = DashboardCalculations.Variation(current.Income, previous.Income),
            ExpenseVariationPct = DashboardCalculations.Variation(current.Expense, previous.Expense),
            TopCategories = top,
            ExpenseEvolution = evolution
        };
    }
}
