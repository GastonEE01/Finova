namespace Finova.Application.DTOs;

public class MovementHistoryResponse : MovementResponse
{
    public string AccountName { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public decimal RunningBalance { get; set; }
}
