namespace Finova.Domain.Entities;

public class SavingGoal
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal TargetAmount { get; set; }
    public DateTime TargetDate { get; set; }
    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = null!;
    public List<Movement> Contributions { get; set; } = new();
}
