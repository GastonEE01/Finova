using Finova.Application.Interfaces;
using Finova.Application.UseCases;
using Finova.Domain.Entities;
using Finova.Domain.Enums;
using FluentAssertions;
using Moq;

namespace Finova.Tests;

public class DashboardUseCasesTests
{
    private readonly Mock<IAccountRepository> _accounts = new();
    private readonly Mock<IMovementRepository> _movements = new();
    private readonly Guid _userId = Guid.NewGuid();

    private Account Acc(string currency, params (MovementType Type, decimal Amount, DateTime Date)[] rows) =>
        new()
        {
            Id = Guid.NewGuid(), UserId = _userId, Name = "C", Currency = currency,
            Movements = rows.Select(r => new Movement { Type = r.Type, Amount = r.Amount, Date = r.Date }).ToList()
        };

    private Movement Row(string currency, MovementType type, decimal amount, DateTime date, string? category = null) =>
        new()
        {
            Id = Guid.NewGuid(),
            AccountId = Guid.NewGuid(),
            Account = new Account { Id = Guid.NewGuid(), Name = "Caja", Currency = currency },
            CategoryId = category is null ? null : Guid.NewGuid(),
            Category = category is null ? null : new Category { Name = category, Type = type },
            Type = type,
            Amount = amount,
            Date = date
        };

    private static DateTime Now() => DateTime.UtcNow;

    [Fact]
    public async Task Get_AgrupaSaldosPorMoneda()
    {
        _accounts.Setup(r => r.ListByUserWithMovementsAsync(_userId)).ReturnsAsync(new List<Account>
        {
            Acc("ARS", (MovementType.Income, 100, Now()), (MovementType.Expense, 20, Now())),
            Acc("USD", (MovementType.Income, 50, Now()))
        });
        _movements.Setup(r => r.ListByUserWithDetailsAsync(_userId)).ReturnsAsync(new List<Movement>());

        var result = await new GetDashboardUseCase(_accounts.Object, _movements.Object).ExecuteAsync(_userId);

        result.TotalBalances.Should().ContainSingle(t => t.Currency == "ARS").Which.Amount.Should().Be(80);
        result.TotalBalances.Should().ContainSingle(t => t.Currency == "USD").Which.Amount.Should().Be(50);
    }

    [Fact]
    public async Task Get_TotalesDelMesActual()
    {
        var now = Now();
        _accounts.Setup(r => r.ListByUserWithMovementsAsync(_userId)).ReturnsAsync(new List<Account>());
        _movements.Setup(r => r.ListByUserWithDetailsAsync(_userId)).ReturnsAsync(new List<Movement>
        {
            Row("ARS", MovementType.Income, 200, now),
            Row("ARS", MovementType.Expense, 60, now),
            Row("ARS", MovementType.Income, 999, now.AddMonths(-2))
        });

        var result = await new GetDashboardUseCase(_accounts.Object, _movements.Object).ExecuteAsync(_userId);

        result.TotalIncome.Should().ContainSingle().Which.Amount.Should().Be(200);
        result.TotalExpenses.Should().ContainSingle().Which.Amount.Should().Be(60);
    }

    [Fact]
    public async Task Get_DevuelveUltimos5()
    {
        var now = Now();
        _accounts.Setup(r => r.ListByUserWithMovementsAsync(_userId)).ReturnsAsync(new List<Account>());
        _movements.Setup(r => r.ListByUserWithDetailsAsync(_userId)).ReturnsAsync(
            Enumerable.Range(1, 7).Select(i => Row("ARS", MovementType.Expense, i, now.AddMinutes(-i))).ToList());

        var result = await new GetDashboardUseCase(_accounts.Object, _movements.Object).ExecuteAsync(_userId);

        result.RecentMovements.Should().HaveCount(5);
    }

    [Fact]
    public async Task ExpensesByCategory_AgrupaSinCategoria()
    {
        var now = Now();
        _movements.Setup(r => r.ListByUserWithDetailsAsync(_userId)).ReturnsAsync(new List<Movement>
        {
            Row("ARS", MovementType.Expense, 40, now),
            Row("ARS", MovementType.Expense, 10, now, "Comida"),
            Row("USD", MovementType.Expense, 500, now)
        });

        var result = await new GetExpensesByCategoryUseCase(_movements.Object).ExecuteAsync(_userId, "ARS");

        result.Items.Should().ContainSingle(i => i.CategoryName == "Sin categoría").Which.Total.Should().Be(40);
        result.Items.Should().ContainSingle(i => i.CategoryName == "Comida").Which.Total.Should().Be(10);
        result.Currency.Should().Be("ARS");
    }

    [Fact]
    public async Task ExpensesByCategory_SoloMesActual()
    {
        var now = Now();
        _movements.Setup(r => r.ListByUserWithDetailsAsync(_userId)).ReturnsAsync(new List<Movement>
        {
            Row("ARS", MovementType.Expense, 40, now),
            Row("ARS", MovementType.Expense, 100, now.AddMonths(-1))
        });

        var result = await new GetExpensesByCategoryUseCase(_movements.Object).ExecuteAsync(_userId, "ARS");

        result.Items.Sum(i => i.Total).Should().Be(40);
    }

    [Fact]
    public async Task ExpensesByCategory_SinGastos_DevuelveVacio()
    {
        _movements.Setup(r => r.ListByUserWithDetailsAsync(_userId)).ReturnsAsync(new List<Movement>());

        var result = await new GetExpensesByCategoryUseCase(_movements.Object).ExecuteAsync(_userId, "ARS");

        result.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task IncomeVsExpenses_Devuelve6MesesConCeros()
    {
        var now = Now();
        _movements.Setup(r => r.ListByUserWithDetailsAsync(_userId)).ReturnsAsync(new List<Movement>
        {
            Row("ARS", MovementType.Income, 100, now)
        });

        var result = await new GetIncomeVsExpensesUseCase(_movements.Object).ExecuteAsync(_userId, "ARS");

        result.Months.Should().HaveCount(6);
        result.Months.Last().Income.Should().Be(100);
        result.Months.Take(5).Sum(m => m.Income + m.Expense).Should().Be(0);
    }

    [Fact]
    public async Task IncomeVsExpenses_FiltraPorMoneda()
    {
        var now = Now();
        _movements.Setup(r => r.ListByUserWithDetailsAsync(_userId)).ReturnsAsync(new List<Movement>
        {
            Row("USD", MovementType.Income, 500, now)
        });

        var result = await new GetIncomeVsExpensesUseCase(_movements.Object).ExecuteAsync(_userId, "ARS");

        result.Months.Sum(m => m.Income + m.Expense).Should().Be(0);
    }

    [Fact]
    public async Task IncomeVsExpenses_MesesEnOrdenCronologico()
    {
        _movements.Setup(r => r.ListByUserWithDetailsAsync(_userId)).ReturnsAsync(new List<Movement>());

        var result = await new GetIncomeVsExpensesUseCase(_movements.Object).ExecuteAsync(_userId, "ARS");

        var keys = result.Months.Select(m => m.Year * 12 + m.Month).ToList();
        keys.Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task BalanceEvolution_SaldoAcumuladoCrece()
    {
        var now = Now();
        var day1 = new DateTime(now.Year, now.Month, 1, 12, 0, 0, DateTimeKind.Utc);
        _movements.Setup(r => r.ListByUserWithDetailsAsync(_userId)).ReturnsAsync(new List<Movement>
        {
            Row("ARS", MovementType.Income, 100, day1),
            Row("ARS", MovementType.Expense, 30, day1.AddDays(1))
        });

        var result = await new GetBalanceEvolutionUseCase(_movements.Object).ExecuteAsync(_userId, "ARS");

        result.Points.Should().HaveCount(now.Day);
        result.Points.First().Balance.Should().Be(100);
        if (now.Day >= 2)
            result.Points[1].Balance.Should().Be(70);
    }

    [Fact]
    public async Task BalanceEvolution_SinMovimientos_PuntosEnCero()
    {
        _movements.Setup(r => r.ListByUserWithDetailsAsync(_userId)).ReturnsAsync(new List<Movement>());

        var result = await new GetBalanceEvolutionUseCase(_movements.Object).ExecuteAsync(_userId, "ARS");

        result.Points.Should().OnlyContain(p => p.Balance == 0);
    }

    [Fact]
    public async Task BalanceEvolution_FiltraPorMoneda()
    {
        var now = Now();
        _movements.Setup(r => r.ListByUserWithDetailsAsync(_userId)).ReturnsAsync(new List<Movement>
        {
            Row("USD", MovementType.Income, 500, new DateTime(now.Year, now.Month, 1, 12, 0, 0, DateTimeKind.Utc))
        });

        var result = await new GetBalanceEvolutionUseCase(_movements.Object).ExecuteAsync(_userId, "ARS");

        result.Points.Should().OnlyContain(p => p.Balance == 0);
    }

    [Fact]
    public async Task Comparisons_VariacionNula_SiMesPrevioEnCero()
    {
        var now = Now();
        _movements.Setup(r => r.ListByUserWithDetailsAsync(_userId)).ReturnsAsync(new List<Movement>
        {
            Row("ARS", MovementType.Expense, 100, now)
        });

        var result = await new GetComparisonsUseCase(_movements.Object).ExecuteAsync(_userId, "ARS");

        result.CurrentMonth.Expense.Should().Be(100);
        result.ExpenseVariationPct.Should().BeNull();
    }

    [Fact]
    public async Task Comparisons_CalculaVariacionPorcentual()
    {
        var now = Now();
        var prev = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-1);
        _movements.Setup(r => r.ListByUserWithDetailsAsync(_userId)).ReturnsAsync(new List<Movement>
        {
            Row("ARS", MovementType.Expense, 150, now),
            Row("ARS", MovementType.Expense, 100, prev.AddDays(5))
        });

        var result = await new GetComparisonsUseCase(_movements.Object).ExecuteAsync(_userId, "ARS");

        result.ExpenseVariationPct.Should().Be(50);
    }

    [Fact]
    public async Task Comparisons_EvolucionSiempre6Meses()
    {
        _movements.Setup(r => r.ListByUserWithDetailsAsync(_userId)).ReturnsAsync(new List<Movement>());

        var result = await new GetComparisonsUseCase(_movements.Object).ExecuteAsync(_userId, "ARS");

        result.ExpenseEvolution.Should().HaveCount(6);
        result.TopCategories.Should().BeEmpty();
    }
}
