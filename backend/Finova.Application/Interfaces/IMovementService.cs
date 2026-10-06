using Finova.Application.DTOs;
using Finova.Domain.Enums;

namespace Finova.Application.Interfaces;

public interface IMovementService
{
    Task<MovementResponse> CreateAsync(Guid userId, CreateMovementRequest request);
    Task<List<MovementResponse>> GetAllByUserAsync(Guid userId);
    Task<List<MovementHistoryResponse>> GetHistoryAsync(Guid userId, DateTime? from, DateTime? to, Guid? accountId, Guid? categoryId, MovementType? type);
}
