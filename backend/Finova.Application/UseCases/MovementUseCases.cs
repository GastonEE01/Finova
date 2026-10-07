using Finova.Application.DTOs;
using Finova.Application.Interfaces;
using Finova.Domain.Entities;
using Finova.Domain.Enums;

namespace Finova.Application.UseCases;

public class CreateMovementUseCase
{
    private readonly IAccountRepository _accounts;
    private readonly ICategoryRepository _categories;
    private readonly IMovementRepository _movements;

    public CreateMovementUseCase(
        IAccountRepository accounts,
        ICategoryRepository categories,
        IMovementRepository movements)
    {
        _accounts = accounts;
        _categories = categories;
        _movements = movements;
    }

    public async Task<MovementResponse> ExecuteAsync(Guid userId, CreateMovementRequest request)
    {
        var account = await _accounts.GetOwnedAsync(userId, request.AccountId);
        if (account is null)
            throw new KeyNotFoundException("Cuenta no encontrada.");

        if (request.Amount <= 0)
            throw new ArgumentException("El monto debe ser mayor a 0.");

        if (request.Type == MovementType.Expense && string.IsNullOrWhiteSpace(request.Description))
            throw new ArgumentException("La descripción es obligatoria para un gasto.");

        string? categoryName = null;
        if (request.CategoryId.HasValue)
        {
            var category = await _categories.GetAccessibleAsync(userId, request.CategoryId.Value);
            if (category is null)
                throw new ArgumentException("Categoría no encontrada.");
            if (category.Type != request.Type)
                throw new ArgumentException("La categoría no coincide con el tipo de movimiento.");
            categoryName = category.Name;
        }

        var movement = new Movement
        {
            Id = Guid.NewGuid(),
            AccountId = request.AccountId,
            CategoryId = request.CategoryId,
            Type = request.Type,
            Amount = request.Amount,
            Date = DateTime.SpecifyKind(request.Date, DateTimeKind.Utc),
            Description = request.Description?.Trim()
        };

        await _movements.AddAsync(movement);

        return new MovementResponse
        {
            Id = movement.Id,
            AccountId = movement.AccountId,
            CategoryId = movement.CategoryId,
            CategoryName = categoryName,
            Type = movement.Type,
            Amount = movement.Amount,
            Date = movement.Date,
            Description = movement.Description
        };
    }
}

public class ListMovementsUseCase
{
    private readonly IMovementRepository _movements;

    public ListMovementsUseCase(IMovementRepository movements)
    {
        _movements = movements;
    }

    public async Task<List<MovementResponse>> ExecuteAsync(Guid userId)
    {
        var movements = await _movements.ListByUserWithDetailsAsync(userId);
        return movements
            .OrderByDescending(m => m.Date)
            .Select(m => new MovementResponse
            {
                Id = m.Id,
                AccountId = m.AccountId,
                CategoryId = m.CategoryId,
                CategoryName = m.Category?.Name,
                Type = m.Type,
                Amount = m.Amount,
                Date = m.Date,
                Description = m.Description
            })
            .ToList();
    }
}

public class GetMovementHistoryUseCase
{
    private readonly IMovementRepository _movements;

    public GetMovementHistoryUseCase(IMovementRepository movements)
    {
        _movements = movements;
    }

    public async Task<List<MovementHistoryResponse>> ExecuteAsync(
        Guid userId, DateTime? from, DateTime? to, Guid? accountId, Guid? categoryId, MovementType? type)
    {
        if (from.HasValue && to.HasValue && from.Value.Date > to.Value.Date)
            throw new ArgumentException("La fecha desde no puede ser posterior a la fecha hasta.");

        var rows = await _movements.ListByUserWithDetailsAsync(userId);

        var balances = new Dictionary<Guid, decimal>();
        var history = rows
            .OrderBy(m => m.Date)
            .ThenBy(m => m.Id)
            .Select(m =>
            {
                var signed = m.Type == MovementType.Income ? m.Amount : -m.Amount;
                balances[m.AccountId] = balances.GetValueOrDefault(m.AccountId) + signed;
                return new MovementHistoryResponse
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
                    RunningBalance = balances[m.AccountId]
                };
            });

        if (from.HasValue)
            history = history.Where(h => h.Date.Date >= from.Value.Date);
        if (to.HasValue)
            history = history.Where(h => h.Date.Date <= to.Value.Date);
        if (accountId.HasValue)
            history = history.Where(h => h.AccountId == accountId.Value);
        if (categoryId.HasValue)
            history = history.Where(h => h.CategoryId == categoryId.Value);
        if (type.HasValue)
            history = history.Where(h => h.Type == type.Value);

        return history.OrderByDescending(h => h.Date).ThenByDescending(h => h.Id).ToList();
    }
}
