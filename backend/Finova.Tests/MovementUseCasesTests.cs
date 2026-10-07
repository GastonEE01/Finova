using Finova.Application.DTOs;
using Finova.Application.Interfaces;
using Finova.Application.UseCases;
using Finova.Domain.Entities;
using Finova.Domain.Enums;
using FluentAssertions;
using Moq;

namespace Finova.Tests;

public class MovementUseCasesTests
{
    private readonly Mock<IAccountRepository> _accounts = new();
    private readonly Mock<ICategoryRepository> _categories = new();
    private readonly Mock<IMovementRepository> _movements = new();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _accountId = Guid.NewGuid();

    private Account OwnedAccount() => new()
    {
        Id = _accountId, UserId = _userId, Name = "Caja", Currency = "ARS"
    };

    private CreateMovementUseCase Create() => new(_accounts.Object, _categories.Object, _movements.Object);

    private Movement Row(Guid accountId, string currency, MovementType type, decimal amount, DateTime date, Guid? categoryId = null, string? categoryName = null) =>
        new()
        {
            Id = Guid.NewGuid(),
            AccountId = accountId,
            Account = new Account { Id = accountId, Name = "Caja", Currency = currency },
            CategoryId = categoryId,
            Category = categoryName is null ? null : new Category { Id = categoryId!.Value, Name = categoryName, Type = type },
            Type = type,
            Amount = amount,
            Date = date
        };

    [Fact]
    public async Task Create_IngresoValido_GuardaYDevuelveRespuesta()
    {
        _accounts.Setup(r => r.GetOwnedAsync(_userId, _accountId)).ReturnsAsync(OwnedAccount());
        Movement? saved = null;
        _movements.Setup(r => r.AddAsync(It.IsAny<Movement>()))
            .Callback<Movement>(m => saved = m).Returns(Task.CompletedTask);

        var result = await Create().ExecuteAsync(_userId, new CreateMovementRequest
        {
            AccountId = _accountId, Type = MovementType.Income, Amount = 100,
            Date = new DateTime(2026, 10, 1), Description = "Sueldo"
        });

        result.Amount.Should().Be(100);
        saved.Should().NotBeNull();
        saved!.AccountId.Should().Be(_accountId);
        saved.Date.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public async Task Create_GastoSinDescripcion_LanzaArgument()
    {
        _accounts.Setup(r => r.GetOwnedAsync(_userId, _accountId)).ReturnsAsync(OwnedAccount());

        var act = () => Create().ExecuteAsync(_userId, new CreateMovementRequest
        {
            AccountId = _accountId, Type = MovementType.Expense, Amount = 10,
            Date = DateTime.UtcNow, Description = "  "
        });

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("La descripción es obligatoria para un gasto.");
    }

    [Fact]
    public async Task Create_CuentaAjena_LanzaNotFound()
    {
        var other = Guid.NewGuid();
        _accounts.Setup(r => r.GetOwnedAsync(other, _accountId)).ReturnsAsync((Account?)null);

        var act = () => Create().ExecuteAsync(other, new CreateMovementRequest
        {
            AccountId = _accountId, Type = MovementType.Income, Amount = 10,
            Date = DateTime.UtcNow, Description = "x"
        });

        await act.Should().ThrowAsync<KeyNotFoundException>();
        _movements.Verify(r => r.AddAsync(It.IsAny<Movement>()), Times.Never);
    }

    [Fact]
    public async Task Create_MontoCero_LanzaArgument()
    {
        _accounts.Setup(r => r.GetOwnedAsync(_userId, _accountId)).ReturnsAsync(OwnedAccount());

        var act = () => Create().ExecuteAsync(_userId, new CreateMovementRequest
        {
            AccountId = _accountId, Type = MovementType.Income, Amount = 0,
            Date = DateTime.UtcNow, Description = "x"
        });

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("El monto debe ser mayor a 0.");
    }

    [Fact]
    public async Task Create_CategoriaTipoDistinto_LanzaArgument()
    {
        var catId = Guid.NewGuid();
        _accounts.Setup(r => r.GetOwnedAsync(_userId, _accountId)).ReturnsAsync(OwnedAccount());
        _categories.Setup(r => r.GetAccessibleAsync(_userId, catId)).ReturnsAsync(
            new Category { Id = catId, Name = "Sueldo", Type = MovementType.Income });

        var act = () => Create().ExecuteAsync(_userId, new CreateMovementRequest
        {
            AccountId = _accountId, CategoryId = catId, Type = MovementType.Expense, Amount = 10,
            Date = DateTime.UtcNow, Description = "gasto"
        });

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("La categoría no coincide con el tipo de movimiento.");
    }

    [Fact]
    public async Task Create_CategoriaAjena_LanzaArgument()
    {
        var catId = Guid.NewGuid();
        _accounts.Setup(r => r.GetOwnedAsync(_userId, _accountId)).ReturnsAsync(OwnedAccount());
        _categories.Setup(r => r.GetAccessibleAsync(_userId, catId)).ReturnsAsync((Category?)null);

        var act = () => Create().ExecuteAsync(_userId, new CreateMovementRequest
        {
            AccountId = _accountId, CategoryId = catId, Type = MovementType.Expense, Amount = 10,
            Date = DateTime.UtcNow, Description = "gasto"
        });

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Categoría no encontrada.");
    }

    [Fact]
    public async Task List_OrdenaPorFechaDesc()
    {
        var old = Row(_accountId, "ARS", MovementType.Income, 10, new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc));
        var recent = Row(_accountId, "ARS", MovementType.Expense, 5, new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc));
        _movements.Setup(r => r.ListByUserWithDetailsAsync(_userId)).ReturnsAsync(new List<Movement> { old, recent });

        var result = await new ListMovementsUseCase(_movements.Object).ExecuteAsync(_userId);

        result.Select(m => m.Amount).Should().ContainInOrder(5, 10);
    }

    [Fact]
    public async Task History_CalculaSaldoAcumulado_PorCuenta()
    {
        var m1 = Row(_accountId, "ARS", MovementType.Income, 100, new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc));
        var m2 = Row(_accountId, "ARS", MovementType.Expense, 30, new DateTime(2026, 10, 2, 0, 0, 0, DateTimeKind.Utc));
        _movements.Setup(r => r.ListByUserWithDetailsAsync(_userId)).ReturnsAsync(new List<Movement> { m2, m1 });

        var result = await new GetMovementHistoryUseCase(_movements.Object)
            .ExecuteAsync(_userId, null, null, null, null, null);

        result.Should().HaveCount(2);
        result.First().RunningBalance.Should().Be(70);
        result.Last().RunningBalance.Should().Be(100);
    }

    [Fact]
    public async Task History_RangoInvertido_LanzaArgument()
    {
        var act = () => new GetMovementHistoryUseCase(_movements.Object).ExecuteAsync(
            _userId, new DateTime(2026, 10, 5), new DateTime(2026, 10, 1), null, null, null);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("La fecha desde no puede ser posterior a la fecha hasta.");
    }
}
