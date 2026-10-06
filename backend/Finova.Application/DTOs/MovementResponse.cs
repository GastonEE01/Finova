using Finova.Domain.Enums;

namespace Finova.Application.DTOs;

public class MovementResponse
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public Guid? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public MovementType Type { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string? Description { get; set; }
}
