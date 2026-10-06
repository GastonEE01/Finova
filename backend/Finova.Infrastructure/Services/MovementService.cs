using Finova.Application.DTOs;
using Finova.Application.Interfaces;
using Finova.Domain.Entities;
using Finova.Domain.Enums;
using Finova.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Finova.Infrastructure.Services;

public class MovementService : IMovementService
{
    private readonly FinovaDbContext _context;

    public MovementService(FinovaDbContext context)
    {
        _context = context;
    }

    public async Task<MovementResponse> CreateAsync(Guid userId, CreateMovementRequest request)
    {
        var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == request.AccountId && a.UserId == userId);
        if (account is null)
            throw new KeyNotFoundException("Cuenta no encontrada.");

        if (request.Amount <= 0)
            throw new ArgumentException("El monto debe ser mayor a 0.");

        if (request.Type == MovementType.Expense)
        {
            if (string.IsNullOrWhiteSpace(request.Description))
                throw new ArgumentException("La descripción es obligatoria para un gasto.");
        }

        if (request.CategoryId.HasValue)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == request.CategoryId && (c.UserId == userId || c.UserId == null));
            if (category is null)
                throw new ArgumentException("Categoría no encontrada.");
            if (category.Type != request.Type)
                throw new ArgumentException("La categoría no coincide con el tipo de movimiento.");
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

        _context.Movements.Add(movement);
        await _context.SaveChangesAsync();

        return new MovementResponse
        {
            Id = movement.Id,
            AccountId = movement.AccountId,
            CategoryId = movement.CategoryId,
            CategoryName = request.CategoryId.HasValue
                ? (await _context.Categories.FindAsync(request.CategoryId.Value))?.Name
                : null,
            Type = movement.Type,
            Amount = movement.Amount,
            Date = movement.Date,
            Description = movement.Description
        };
    }

    public async Task<List<MovementResponse>> GetAllByUserAsync(Guid userId)
    {
        return await _context.Movements
            .Where(m => m.Account.UserId == userId)
            .OrderByDescending(m => m.Date)
            .Select(m => new MovementResponse
            {
                Id = m.Id,
                AccountId = m.AccountId,
                CategoryId = m.CategoryId,
                CategoryName = m.Category != null ? m.Category.Name : null,
                Type = m.Type,
                Amount = m.Amount,
                Date = m.Date,
                Description = m.Description
            })
            .ToListAsync();
    }

    public async Task<List<MovementHistoryResponse>> GetHistoryAsync(Guid userId, DateTime? from, DateTime? to, Guid? accountId, Guid? categoryId, MovementType? type)
    {
        if (from.HasValue && to.HasValue && from.Value.Date > to.Value.Date)
            throw new ArgumentException("La fecha desde no puede ser posterior a la fecha hasta.");

        var rows = await _context.Movements
            .Where(m => m.Account.UserId == userId)
            .OrderBy(m => m.Date)
            .ThenBy(m => m.Id)
            .Select(m => new
            {
                m.Id,
                m.AccountId,
                AccountName = m.Account.Name,
                Currency = m.Account.Currency,
                m.CategoryId,
                CategoryName = m.Category != null ? m.Category.Name : null,
                m.Type,
                m.Amount,
                m.Date,
                m.Description
            })
            .ToListAsync();

        var balances = new Dictionary<Guid, decimal>();
        var history = rows.Select(r =>
        {
            var signed = r.Type == MovementType.Income ? r.Amount : -r.Amount;
            balances[r.AccountId] = balances.GetValueOrDefault(r.AccountId) + signed;
            return new MovementHistoryResponse
            {
                Id = r.Id,
                AccountId = r.AccountId,
                AccountName = r.AccountName,
                Currency = r.Currency,
                CategoryId = r.CategoryId,
                CategoryName = r.CategoryName,
                Type = r.Type,
                Amount = r.Amount,
                Date = r.Date,
                Description = r.Description,
                RunningBalance = balances[r.AccountId]
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
