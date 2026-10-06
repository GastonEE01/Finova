namespace Finova.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public List<Account> Accounts { get; set; } = new();
    public List<Category> Categories { get; set; } = new();
    public List<Budget> Budgets { get; set; } = new();
    public List<SavingGoal> SavingGoals { get; set; } = new();
}
