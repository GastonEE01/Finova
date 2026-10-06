using Finova.Domain.Enums;

namespace Finova.Domain.Entities;

public class Movement
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public Guid? CategoryId { get; set; }
    public MovementType Type { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string? Description { get; set; }
    public Guid? GoalId { get; set; }

    public Account Account { get; set; } = null!;
    public Category? Category { get; set; }
    public SavingGoal? SavingGoal { get; set; }
}
