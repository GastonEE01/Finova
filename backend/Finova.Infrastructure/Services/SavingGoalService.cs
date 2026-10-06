using Finova.Application.DTOs;
using Finova.Application.Interfaces;
using Finova.Domain.Entities;
using Finova.Domain.Enums;
using Finova.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Finova.Infrastructure.Services;

public class SavingGoalService : ISavingGoalService
{
    private readonly FinovaDbContext _context;

    public SavingGoalService(FinovaDbContext context)
    {
        _context = context;
    }

    public async Task<List<SavingGoalResponse>> GetAllAsync(Guid userId)
    {
        var goals = await _context.SavingGoals
            .Where(g => g.UserId == userId)
            .OrderBy(g => g.TargetDate)
            .ToListAsync();
        var result = new List<SavingGoalResponse>();
        foreach (var g in goals)
            result.Add(await MapAsync(g));
        return result;
    }

    public async Task<SavingGoalResponse> CreateAsync(Guid userId, CreateSavingGoalRequest request)
    {
        var name = ValidateGoal(request.Name, request.TargetAmount, request.TargetDate);
        var goal = new SavingGoal
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = name,
            TargetAmount = request.TargetAmount,
            TargetDate = DateTime.SpecifyKind(request.TargetDate.Date, DateTimeKind.Utc),
            CreatedAt = DateTime.UtcNow
        };
        _context.SavingGoals.Add(goal);
        await _context.SaveChangesAsync();
        return await MapAsync(goal);
    }

    public async Task<SavingGoalResponse> UpdateAsync(Guid userId, Guid id, UpdateSavingGoalRequest request)
    {
        var goal = await _context.SavingGoals.FirstOrDefaultAsync(g => g.Id == id && g.UserId == userId);
        if (goal is null)
            throw new KeyNotFoundException("Meta no encontrada.");
        var name = ValidateGoal(request.Name, request.TargetAmount, request.TargetDate);
        goal.Name = name;
        goal.TargetAmount = request.TargetAmount;
        goal.TargetDate = DateTime.SpecifyKind(request.TargetDate.Date, DateTimeKind.Utc);
        await _context.SaveChangesAsync();
        return await MapAsync(goal);
    }

    public async Task DeleteAsync(Guid userId, Guid id)
    {
        var goal = await _context.SavingGoals.FirstOrDefaultAsync(g => g.Id == id && g.UserId == userId);
        if (goal is null)
            throw new KeyNotFoundException("Meta no encontrada.");
        _context.SavingGoals.Remove(goal);
        await _context.SaveChangesAsync();
    }

    public async Task<ContributionResponse> AddContributionAsync(Guid userId, Guid goalId, AddContributionRequest request)
    {
        var goal = await _context.SavingGoals.FirstOrDefaultAsync(g => g.Id == goalId && g.UserId == userId);
        if (goal is null)
            throw new KeyNotFoundException("Meta no encontrada.");
        if (request.Amount <= 0)
            throw new ArgumentException("El monto debe ser mayor a 0.");

        var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == request.AccountId && a.UserId == userId);
        if (account is null)
            throw new KeyNotFoundException("Cuenta no encontrada.");

        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == request.CategoryId && (c.UserId == userId || c.UserId == null));
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

        _context.Movements.Add(movement);
        await _context.SaveChangesAsync();

        var mapped = await MapAsync(goal);
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

    private static string ValidateGoal(string name, decimal targetAmount, DateTime targetDate)
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

    private async Task<SavingGoalResponse> MapAsync(SavingGoal goal)
    {
        var progress = await _context.Movements
            .Where(m => m.GoalId == goal.Id)
            .SumAsync(m => (decimal?)m.Amount) ?? 0;
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
