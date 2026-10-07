using Finova.Application.DTOs;
using Finova.Application.Interfaces;
using Finova.Application.UseCases;
using Finova.Domain.Entities;
using Finova.Domain.Enums;
using FluentAssertions;
using Moq;

namespace Finova.Tests;

public class BudgetUseCasesTests
{
    private readonly Mock<IBudgetRepository> _budgets = new();
    private readonly Mock<ICategoryRepository> _categories = new();
    private readonly Mock<IMovementRepository> _movements = new();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _categoryId = Guid.NewGuid();

    private Category ExpenseCategory() => new()
    {
        Id = _categoryId, Name = "Comida", Type = MovementType.Expense
    };

    private Budget Owned(decimal amount = 100) => new()
    {
        Id = Guid.NewGuid(), UserId = _userId, CategoryId = _categoryId,
        Year = 2026, Month = 10, Amount = amount, Currency = "ARS", Category = ExpenseCategory()
    };

    [Fact]
    public async Task Create_Valido_GuardaYCalculaEstado()
    {
        _categories.Setup(r => r.GetAccessibleAsync(_userId, _categoryId)).ReturnsAsync(ExpenseCategory());
        _budgets.Setup(r => r.ExistsAsync(_userId, _categoryId, 2026, 10, "ARS")).ReturnsAsync(false);
        _movements.Setup(r => r.SumExpensesAsync(_userId, _categoryId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), "ARS"))
            .ReturnsAsync(85);

        var result = await new CreateBudgetUseCase(_budgets.Object, _categories.Object, _movements.Object)
            .ExecuteAsync(_userId, new CreateBudgetRequest
            {
                CategoryId = _categoryId, Year = 2026, Month = 10, Amount = 100, Currency = "ars"
            });

        result.Currency.Should().Be("ARS");
        result.Spent.Should().Be(85);
        result.Percent.Should().Be(85);
        result.Status.Should().Be("Acercandose");
        _budgets.Verify(r => r.AddAsync(It.Is<Budget>(b => b.UserId == _userId)), Times.Once);
    }

    [Fact]
    public async Task Create_Duplicado_LanzaArgument()
    {
        _categories.Setup(r => r.GetAccessibleAsync(_userId, _categoryId)).ReturnsAsync(ExpenseCategory());
        _budgets.Setup(r => r.ExistsAsync(_userId, _categoryId, 2026, 10, "ARS")).ReturnsAsync(true);

        var act = () => new CreateBudgetUseCase(_budgets.Object, _categories.Object, _movements.Object)
            .ExecuteAsync(_userId, new CreateBudgetRequest
            {
                CategoryId = _categoryId, Year = 2026, Month = 10, Amount = 100, Currency = "ARS"
            });

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Ya existe un presupuesto para esa categoría, mes y moneda.");
    }

    [Fact]
    public async Task Create_CategoriaIngreso_LanzaArgument()
    {
        _categories.Setup(r => r.GetAccessibleAsync(_userId, _categoryId)).ReturnsAsync(
            new Category { Id = _categoryId, Name = "Sueldo", Type = MovementType.Income });

        var act = () => new CreateBudgetUseCase(_budgets.Object, _categories.Object, _movements.Object)
            .ExecuteAsync(_userId, new CreateBudgetRequest
            {
                CategoryId = _categoryId, Year = 2026, Month = 10, Amount = 100, Currency = "ARS"
            });

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("El presupuesto solo aplica a categorías de gasto.");
    }

    [Fact]
    public async Task GetByMonth_MapeaGastadoYEstado()
    {
        var budget = Owned();
        _budgets.Setup(r => r.ListByMonthAsync(_userId, 2026, 10)).ReturnsAsync(new List<Budget> { budget });
        _movements.Setup(r => r.SumExpensesAsync(_userId, _categoryId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), "ARS"))
            .ReturnsAsync(120);

        var result = await new GetBudgetsByMonthUseCase(_budgets.Object, _movements.Object)
            .ExecuteAsync(_userId, 2026, 10);

        result.Should().ContainSingle().Which.Should().Match<BudgetResponse>(b =>
            b.Spent == 120 && b.Remaining == -20 && b.Status == "Superado");
    }

    [Fact]
    public async Task GetByMonth_SinPresupuestos_DevuelveVacio()
    {
        _budgets.Setup(r => r.ListByMonthAsync(_userId, 2026, 10)).ReturnsAsync(new List<Budget>());

        var result = await new GetBudgetsByMonthUseCase(_budgets.Object, _movements.Object)
            .ExecuteAsync(_userId, 2026, 10);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByMonth_SoloPideDatosDelUsuarioYMes()
    {
        _budgets.Setup(r => r.ListByMonthAsync(_userId, 2026, 10)).ReturnsAsync(new List<Budget>());

        await new GetBudgetsByMonthUseCase(_budgets.Object, _movements.Object).ExecuteAsync(_userId, 2026, 10);

        _budgets.Verify(r => r.ListByMonthAsync(_userId, 2026, 10), Times.Once);
    }

    [Fact]
    public async Task Update_MontoValido_Actualiza()
    {
        var budget = Owned();
        _budgets.Setup(r => r.GetOwnedAsync(_userId, budget.Id)).ReturnsAsync(budget);
        _movements.Setup(r => r.SumExpensesAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<string>()))
            .ReturnsAsync(0);

        var result = await new UpdateBudgetUseCase(_budgets.Object, _movements.Object)
            .ExecuteAsync(_userId, budget.Id, new UpdateBudgetRequest { Amount = 200 });

        result.Amount.Should().Be(200);
        _budgets.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Update_Ajeno_LanzaNotFound()
    {
        var id = Guid.NewGuid();
        _budgets.Setup(r => r.GetOwnedAsync(_userId, id)).ReturnsAsync((Budget?)null);

        var act = () => new UpdateBudgetUseCase(_budgets.Object, _movements.Object)
            .ExecuteAsync(_userId, id, new UpdateBudgetRequest { Amount = 200 });

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Update_MontoCero_LanzaArgument()
    {
        var act = () => new UpdateBudgetUseCase(_budgets.Object, _movements.Object)
            .ExecuteAsync(_userId, Guid.NewGuid(), new UpdateBudgetRequest { Amount = 0 });

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("El monto debe ser mayor a 0.");
    }

    [Fact]
    public async Task Delete_Propio_Elimina()
    {
        var budget = Owned();
        _budgets.Setup(r => r.GetOwnedAsync(_userId, budget.Id)).ReturnsAsync(budget);

        await new DeleteBudgetUseCase(_budgets.Object).ExecuteAsync(_userId, budget.Id);

        _budgets.Verify(r => r.Remove(budget), Times.Once);
        _budgets.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Delete_Ajeno_LanzaNotFound()
    {
        var id = Guid.NewGuid();
        _budgets.Setup(r => r.GetOwnedAsync(_userId, id)).ReturnsAsync((Budget?)null);

        var act = () => new DeleteBudgetUseCase(_budgets.Object).ExecuteAsync(_userId, id);

        await act.Should().ThrowAsync<KeyNotFoundException>();
        _budgets.Verify(r => r.Remove(It.IsAny<Budget>()), Times.Never);
    }

    [Fact]
    public async Task Delete_Inexistente_NoGuardaCambios()
    {
        var id = Guid.NewGuid();
        _budgets.Setup(r => r.GetOwnedAsync(_userId, id)).ReturnsAsync((Budget?)null);

        var act = () => new DeleteBudgetUseCase(_budgets.Object).ExecuteAsync(_userId, id);

        await act.Should().ThrowAsync<KeyNotFoundException>();
        _budgets.Verify(r => r.SaveChangesAsync(), Times.Never);
    }
}
