using Finova.Application.DTOs;
using Finova.Application.Interfaces;
using Finova.Domain.Enums;
using Finova.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Finova.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly FinovaDbContext _context;

    public DashboardService(FinovaDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardResponse> GetAsync(Guid userId)
    {
        var now = DateTime.UtcNow;
        var from = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        // Saldo total por moneda: suma de saldos de cuentas (ingresos - gastos).
        var balances = await _context.Accounts
            .Where(a => a.UserId == userId)
            .Select(a => new
            {
                a.Currency,
                Balance = a.Movements.Sum(m => m.Type == MovementType.Income ? m.Amount : -m.Amount)
            })
            .ToListAsync();

        var totalBalances = balances
            .GroupBy(b => b.Currency)
            .Select(g => new CurrencyTotal { Currency = g.Key, Amount = g.Sum(x => x.Balance) })
            .ToList();

        // Totales del mes calendario en UTC (día 1 → ahora, futuros del mes incluidos).
        var monthMovements = await _context.Movements
            .Where(m => m.Account.UserId == userId
                && m.Date.Year == now.Year
                && m.Date.Month == now.Month)
            .Select(m => new { m.Type, Currency = m.Account.Currency, m.Amount })
            .ToListAsync();

        var totalIncome = monthMovements
            .Where(m => m.Type == MovementType.Income)
            .GroupBy(m => m.Currency)
            .Select(g => new CurrencyTotal { Currency = g.Key, Amount = g.Sum(x => x.Amount) })
            .ToList();

        var totalExpenses = monthMovements
            .Where(m => m.Type == MovementType.Expense)
            .GroupBy(m => m.Currency)
            .Select(g => new CurrencyTotal { Currency = g.Key, Amount = g.Sum(x => x.Amount) })
            .ToList();

        // Últimos 5 movimientos. Se reutiliza MovementHistoryResponse;
        // RunningBalance no aplica al dashboard y se devuelve en 0.
        var recent = await _context.Movements
            .Where(m => m.Account.UserId == userId)
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
                CategoryName = m.Category != null ? m.Category.Name : null,
                Type = m.Type,
                Amount = m.Amount,
                Date = m.Date,
                Description = m.Description,
                RunningBalance = 0
            })
            .ToListAsync();

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

    public async Task<ExpensesByCategoryResponse> GetExpensesByCategoryAsync(Guid userId, string currency)
    {
        var now = DateTime.UtcNow;
        var from = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var groups = await _context.Movements
            .Where(m => m.Account.UserId == userId
                && m.Account.Currency == currency
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
            .ToListAsync();

        return new ExpensesByCategoryResponse
        {
            Currency = currency,
            PeriodFrom = from,
            PeriodTo = now,
            Items = groups
        };
    }

    public async Task<IncomeVsExpensesResponse> GetIncomeVsExpensesAsync(Guid userId, string currency)
    {
        var now = DateTime.UtcNow;
        var start = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-5);

        var movements = await _context.Movements
            .Where(m => m.Account.UserId == userId
                && m.Account.Currency == currency
                && m.Date >= start)
            .Select(m => new { m.Type, m.Amount, m.Date })
            .ToListAsync();

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

    public async Task<BalanceEvolutionResponse> GetBalanceEvolutionAsync(Guid userId, string currency)
    {
        var now = DateTime.UtcNow;
        var from = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var movements = await _context.Movements
            .Where(m => m.Account.UserId == userId
                && m.Account.Currency == currency
                && m.Date <= now)
            .Select(m => new { m.Type, m.Amount, m.Date })
            .ToListAsync();

        var points = new List<BalancePointItem>();
        for (var day = 1; day <= now.Day; day++)
        {
            var cutoff = new DateTime(now.Year, now.Month, day, 23, 59, 59, DateTimeKind.Utc);
            var balance = movements
                .Where(m => m.Date <= cutoff)
                .Sum(m => m.Type == MovementType.Income ? m.Amount : -m.Amount);
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

    public async Task<ComparisonsResponse> GetComparisonsAsync(Guid userId, string currency)
    {
        var now = DateTime.UtcNow;
        var curStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var prevStart = curStart.AddMonths(-1);
        var evoStart = curStart.AddMonths(-5);

        var movements = await _context.Movements
            .Where(m => m.Account.UserId == userId
                && m.Account.Currency == currency
                && m.Date >= evoStart)
            .Select(m => new
            {
                m.Type,
                m.Amount,
                m.Date,
                m.CategoryId,
                CategoryName = m.Category != null ? m.Category.Name : null
            })
            .ToListAsync();

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

        static decimal? Variation(decimal cur, decimal prev) =>
            prev == 0 ? null : Math.Round((cur - prev) / prev * 100, 2);

        var currentExpenses = movements
            .Where(m => m.Type == MovementType.Expense && m.Date.Year == now.Year && m.Date.Month == now.Month)
            .ToList();
        var monthTotal = currentExpenses.Sum(m => m.Amount);
        var top = currentExpenses
            .GroupBy(m => new { m.CategoryId, Name = m.CategoryName ?? "Sin categoría" })
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
            IncomeVariationPct = Variation(current.Income, previous.Income),
            ExpenseVariationPct = Variation(current.Expense, previous.Expense),
            TopCategories = top,
            ExpenseEvolution = evolution
        };
    }
}
