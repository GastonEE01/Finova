using Finova.Application.DTOs;
using Finova.Application.Interfaces;
using Finova.Domain.Entities;
using Finova.Domain.Enums;

namespace Finova.Application.UseCases;

public static class SavingGoalMapping
{
    public static string ValidateGoal(string name, decimal targetAmount, DateTime targetDate)
    {
        var trimmed = (name ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
            throw new ArgumentException("El nombre es obligatorio.");
        if (trimmed.Length > 100)
            throw new ArgumentException("El nombre no puede superar los 100 caracteres.");
        if (targetAmount <= 0)
            throw new ArgumentException("El monto objetivo debe ser mayor a 0.");
        if (targetDate == default)
            throw new ArgumentException("La fecha objetivo es obligatoria.");
        if (targetDate.Date <= DateTime.UtcNow.Date)
            throw new ArgumentException("La fecha objetivo debe ser futura.");
        return trimmed;
    }

    public static SavingGoalResponse ToResponse(SavingGoal goal, decimal progress)
    {
        var percent = goal.TargetAmount > 0 ? Math.Round(progress / goal.TargetAmount * 100, 2) : 0;
        string status = progress >= goal.TargetAmount
            ? "Cumplida"
            : goal.TargetDate.Date < DateTime.UtcNow.Date ? "Vencida" : "En curso";

        return new SavingGoalResponse
        {
            Id = goal.Id,
            Name = goal.Name,
            TargetAmount = goal.TargetAmount,
            Progress = progress,
            Remaining = goal.TargetAmount - progress,
            Percent = percent,
            Status = status,
            TargetDate = goal.TargetDate,
            CreatedAt = goal.CreatedAt
        };
    }
}

public class ListSavingGoalsUseCase
{
    private readonly ISavingGoalRepository _goals;
    private readonly IMovementRepository _movements;

    public ListSavingGoalsUseCase(ISavingGoalRepository goals, IMovementRepository movements)
    {
        _goals = goals;
        _movements = movements;
    }

    public async Task<List<SavingGoalResponse>> ExecuteAsync(Guid userId)
    {
        var goals = await _goals.ListByUserAsync(userId);
        var result = new List<SavingGoalResponse>();
        foreach (var g in goals)
            result.Add(SavingGoalMapping.ToResponse(g, await _movements.SumByGoalAsync(g.Id)));
        return result;
    }
}

public class CreateSavingGoalUseCase
{
    private readonly ISavingGoalRepository _goals;
    private readonly IMovementRepository _movements;

    public CreateSavingGoalUseCase(ISavingGoalRepository goals, IMovementRepository movements)
    {
        _goals = goals;
        _movements = movements;
    }

    public async Task<SavingGoalResponse> ExecuteAsync(Guid userId, CreateSavingGoalRequest request)
    {
        var name = SavingGoalMapping.ValidateGoal(request.Name, request.TargetAmount, request.TargetDate);
        var goal = new SavingGoal
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = name,
            TargetAmount = request.TargetAmount,
            TargetDate = DateTime.SpecifyKind(request.TargetDate.Date, DateTimeKind.Utc),
            CreatedAt = DateTime.UtcNow
        };
        await _goals.AddAsync(goal);
        return SavingGoalMapping.ToResponse(goal, await _movements.SumByGoalAsync(goal.Id));
    }
}

public class UpdateSavingGoalUseCase
{
    private readonly ISavingGoalRepository _goals;
    private readonly IMovementRepository _movements;

    public UpdateSavingGoalUseCase(ISavingGoalRepository goals, IMovementRepository movements)
    {
        _goals = goals;
        _movements = movements;
    }

    public async Task<SavingGoalResponse> ExecuteAsync(Guid userId, Guid id, UpdateSavingGoalRequest request)
    {
        var goal = await _goals.GetOwnedAsync(userId, id);
        if (goal is null)
            throw new KeyNotFoundException("Meta no encontrada.");
        var name = SavingGoalMapping.ValidateGoal(request.Name, request.TargetAmount, request.TargetDate);
        goal.Name = name;
        goal.TargetAmount = request.TargetAmount;
        goal.TargetDate = DateTime.SpecifyKind(request.TargetDate.Date, DateTimeKind.Utc);
        await _goals.SaveChangesAsync();
        return SavingGoalMapping.ToResponse(goal, await _movements.SumByGoalAsync(goal.Id));
    }
}

public class DeleteSavingGoalUseCase
{
    private readonly ISavingGoalRepository _goals;

    public DeleteSavingGoalUseCase(ISavingGoalRepository goals)
    {
        _goals = goals;
    }

    public async Task ExecuteAsync(Guid userId, Guid id)
    {
        var goal = await _goals.GetOwnedAsync(userId, id);
        if (goal is null)
            throw new KeyNotFoundException("Meta no encontrada.");
        _goals.Remove(goal);
        await _goals.SaveChangesAsync();
    }
}

public class AddGoalContributionUseCase
{
    private readonly ISavingGoalRepository _goals;
    private readonly IAccountRepository _accounts;
    private readonly ICategoryRepository _categories;
    private readonly IMovementRepository _movements;

    public AddGoalContributionUseCase(
        ISavingGoalRepository goals,
        IAccountRepository accounts,
        ICategoryRepository categories,
        IMovementRepository movements)
    {
        _goals = goals;
        _accounts = accounts;
        _categories = categories;
        _movements = movements;
    }

    public async Task<ContributionResponse> ExecuteAsync(Guid userId, Guid goalId, AddContributionRequest request)
    {
        var goal = await _goals.GetOwnedAsync(userId, goalId);
        if (goal is null)
            throw new KeyNotFoundException("Meta no encontrada.");
        if (request.Amount <= 0)
            throw new ArgumentException("El monto debe ser mayor a 0.");

        var account = await _accounts.GetOwnedAsync(userId, request.AccountId);
        if (account is null)
            throw new KeyNotFoundException("Cuenta no encontrada.");

        var category = await _categories.GetAccessibleAsync(userId, request.CategoryId);
        if (category is null)
            throw new ArgumentException("Categoría no encontrada.");
        if (category.Type != MovementType.Expense)
            throw new ArgumentException("La categoría del aporte debe ser de gasto.");

        var movement = new Movement
        {
            Id = Guid.NewGuid(),
            AccountId = request.AccountId,
            CategoryId = request.CategoryId,
            Type = MovementType.Expense,
            Amount = request.Amount,
            Date = DateTime.UtcNow,
            Description = $"Aporte a meta {goal.Name}",
            GoalId = goal.Id
        };

        await _movements.AddAsync(movement);

        var mapped = SavingGoalMapping.ToResponse(goal, await _movements.SumByGoalAsync(goal.Id));
        return new ContributionResponse
        {
            Id = mapped.Id,
            Name = mapped.Name,
            TargetAmount = mapped.TargetAmount,
            Progress = mapped.Progress,
            Remaining = mapped.Remaining,
            Percent = mapped.Percent,
            Status = mapped.Status,
            TargetDate = mapped.TargetDate,
            CreatedAt = mapped.CreatedAt,
            MovementId = movement.Id
        };
    }
}
