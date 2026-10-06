using Finova.Domain.Enums;

namespace Finova.Domain.Entities;

public class Category
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public MovementType Type { get; set; }

    public User? User { get; set; } = null!;
    public List<Movement> Movements { get; set; } = new();
    public List<Budget> Budgets { get; set; } = new();
}
