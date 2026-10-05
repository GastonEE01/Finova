namespace Finova.Domain.Entities;

public class Account
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Currency { get; set; } = "ARS";

    public User User { get; set; } = null!;
    public List<Movement> Movements { get; set; } = new();
}
