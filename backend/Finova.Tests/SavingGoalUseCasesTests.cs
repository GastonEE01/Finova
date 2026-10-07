using Finova.Application.DTOs;
using Finova.Application.Interfaces;
using Finova.Application.UseCases;
using Finova.Domain.Entities;
using Finova.Domain.Enums;
using FluentAssertions;
using Moq;

namespace Finova.Tests;

public class SavingGoalUseCasesTests
{
    private readonly Mock<ISavingGoalRepository> _goals = new();
    private readonly Mock<IAccountRepository> _accounts = new();
    private readonly Mock<ICategoryRepository> _categories = new();
    private readonly Mock<IMovementRepository> _movements = new();
    private readonly Guid _userId = Guid.NewGuid();

    private SavingGoal Owned(string name = "Viaje") => new()
    {
        Id = Guid.NewGuid(), UserId = _userId, Name = name,
        TargetAmount = 1000, TargetDate = DateTime.UtcNow.AddDays(30), CreatedAt = DateTime.UtcNow
    };

    private static DateTime Future() => DateTime.UtcNow.AddDays(30);

    [Fact]
    public async Task Create_Valida_CreaConProgresoCero()
    {
        _movements.Setup(r => r.SumByGoalAsync(It.IsAny<Guid>())).ReturnsAsync(0);

        var result = await new CreateSavingGoalUseCase(_goals.Object, _movements.Object)
            .ExecuteAsync(_userId, new CreateSavingGoalRequest
            {
                Name = "  Viaje ", TargetAmount = 1000, TargetDate = Future()
            });

        result.Name.Should().Be("Viaje");
        result.Progress.Should().Be(0);
        result.Status.Should().Be("En curso");
        _goals.Verify(r => r.AddAsync(It.Is<SavingGoal>(g => g.UserId == _userId)), Times.Once);
    }

    [Fact]
    public async Task Create_FechaPasada_LanzaArgument()
    {
        var act = () => new CreateSavingGoalUseCase(_goals.Object, _movements.Object)
            .ExecuteAsync(_userId, new CreateSavingGoalRequest
            {
                Name = "Viaje", TargetAmount = 1000, TargetDate = DateTime.UtcNow.AddDays(-1)
            });

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("La fecha objetivo debe ser futura.");
    }

    [Fact]
    public async Task Create_NombreVacio_LanzaArgument()
    {
        var act = () => new CreateSavingGoalUseCase(_goals.Object, _movements.Object)
            .ExecuteAsync(_userId, new CreateSavingGoalRequest
            {
                Name = "  ", TargetAmount = 1000, TargetDate = Future()
            });

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("El nombre es obligatorio.");
    }

    [Fact]
    public async Task List_CalculaProgresoYEstado()
    {
        var goal = Owned();
        _goals.Setup(r => r.ListByUserAsync(_userId)).ReturnsAsync(new List<SavingGoal> { goal });
        _movements.Setup(r => r.SumByGoalAsync(goal.Id)).ReturnsAsync(250);

        var result = await new ListSavingGoalsUseCase(_goals.Object, _movements.Object).ExecuteAsync(_userId);

        result.Should().ContainSingle().Which.Should().Match<SavingGoalResponse>(g =>
            g.Progress == 250 && g.Remaining == 750 && g.Percent == 25 && g.Status == "En curso");
    }

    [Fact]
    public async Task List_MetaCumplida_EstadoCumplida()
    {
        var goal = Owned();
        _goals.Setup(r => r.ListByUserAsync(_userId)).ReturnsAsync(new List<SavingGoal> { goal });
        _movements.Setup(r => r.SumByGoalAsync(goal.Id)).ReturnsAsync(1000);

        var result = await new ListSavingGoalsUseCase(_goals.Object, _movements.Object).ExecuteAsync(_userId);

        result.Should().ContainSingle().Which.Status.Should().Be("Cumplida");
    }

    [Fact]
    public async Task List_SoloPideDatosDelUsuario()
    {
        _goals.Setup(r => r.ListByUserAsync(_userId)).ReturnsAsync(new List<SavingGoal>());

        await new ListSavingGoalsUseCase(_goals.Object, _movements.Object).ExecuteAsync(_userId);

        _goals.Verify(r => r.ListByUserAsync(_userId), Times.Once);
    }

    [Fact]
    public async Task Update_Propia_Actualiza()
    {
        var goal = Owned();
        _goals.Setup(r => r.GetOwnedAsync(_userId, goal.Id)).ReturnsAsync(goal);
        _movements.Setup(r => r.SumByGoalAsync(goal.Id)).ReturnsAsync(0);

        var result = await new UpdateSavingGoalUseCase(_goals.Object, _movements.Object)
            .ExecuteAsync(_userId, goal.Id, new UpdateSavingGoalRequest
            {
                Name = "Auto", TargetAmount = 5000, TargetDate = Future()
            });

        result.Name.Should().Be("Auto");
        _goals.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Update_Ajena_LanzaNotFound()
    {
        var id = Guid.NewGuid();
        _goals.Setup(r => r.GetOwnedAsync(_userId, id)).ReturnsAsync((SavingGoal?)null);

        var act = () => new UpdateSavingGoalUseCase(_goals.Object, _movements.Object)
            .ExecuteAsync(_userId, id, new UpdateSavingGoalRequest
            {
                Name = "Auto", TargetAmount = 5000, TargetDate = Future()
            });

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Delete_Ajena_LanzaNotFound_NoElimina()
    {
        var id = Guid.NewGuid();
        _goals.Setup(r => r.GetOwnedAsync(_userId, id)).ReturnsAsync((SavingGoal?)null);

        var act = () => new DeleteSavingGoalUseCase(_goals.Object).ExecuteAsync(_userId, id);

        await act.Should().ThrowAsync<KeyNotFoundException>();
        _goals.Verify(r => r.Remove(It.IsAny<SavingGoal>()), Times.Never);
    }

    [Fact]
    public async Task Contribute_Valido_CreaGastoVinculado()
    {
        var goal = Owned();
        var accountId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        _goals.Setup(r => r.GetOwnedAsync(_userId, goal.Id)).ReturnsAsync(goal);
        _accounts.Setup(r => r.GetOwnedAsync(_userId, accountId)).ReturnsAsync(
            new Account { Id = accountId, UserId = _userId, Name = "Caja", Currency = "ARS" });
        _categories.Setup(r => r.GetAccessibleAsync(_userId, categoryId)).ReturnsAsync(
            new Category { Id = categoryId, Name = "Ahorro", Type = MovementType.Expense });
        Movement? saved = null;
        _movements.Setup(r => r.AddAsync(It.IsAny<Movement>()))
            .Callback<Movement>(m => saved = m).Returns(Task.CompletedTask);
        _movements.Setup(r => r.SumByGoalAsync(goal.Id)).ReturnsAsync(200);

        var result = await new AddGoalContributionUseCase(_goals.Object, _accounts.Object, _categories.Object, _movements.Object)
            .ExecuteAsync(_userId, goal.Id, new AddContributionRequest
            {
                AccountId = accountId, CategoryId = categoryId, Amount = 200
            });

        saved.Should().NotBeNull();
        saved!.Type.Should().Be(MovementType.Expense);
        saved.GoalId.Should().Be(goal.Id);
        saved.Description.Should().Contain(goal.Name);
        result.MovementId.Should().Be(saved.Id);
        result.Progress.Should().Be(200);
    }

    [Fact]
    public async Task Contribute_CuentaAjena_LanzaNotFound()
    {
        var goal = Owned();
        var accountId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        _goals.Setup(r => r.GetOwnedAsync(_userId, goal.Id)).ReturnsAsync(goal);
        _accounts.Setup(r => r.GetOwnedAsync(_userId, accountId)).ReturnsAsync((Account?)null);

        var act = () => new AddGoalContributionUseCase(_goals.Object, _accounts.Object, _categories.Object, _movements.Object)
            .ExecuteAsync(_userId, goal.Id, new AddContributionRequest
            {
                AccountId = accountId, CategoryId = categoryId, Amount = 200
            });

        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("Cuenta no encontrada.");
    }

    [Fact]
    public async Task Contribute_CategoriaIngreso_LanzaArgument()
    {
        var goal = Owned();
        var accountId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        _goals.Setup(r => r.GetOwnedAsync(_userId, goal.Id)).ReturnsAsync(goal);
        _accounts.Setup(r => r.GetOwnedAsync(_userId, accountId)).ReturnsAsync(
            new Account { Id = accountId, UserId = _userId, Name = "Caja", Currency = "ARS" });
        _categories.Setup(r => r.GetAccessibleAsync(_userId, categoryId)).ReturnsAsync(
            new Category { Id = categoryId, Name = "Sueldo", Type = MovementType.Income });

        var act = () => new AddGoalContributionUseCase(_goals.Object, _accounts.Object, _categories.Object, _movements.Object)
            .ExecuteAsync(_userId, goal.Id, new AddContributionRequest
            {
                AccountId = accountId, CategoryId = categoryId, Amount = 200
            });

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("La categoría del aporte debe ser de gasto.");
    }
}
