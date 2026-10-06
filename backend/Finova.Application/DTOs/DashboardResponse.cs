namespace Finova.Application.DTOs;

public class CurrencyTotal
{
    public string Currency { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class DashboardResponse
{
    public List<CurrencyTotal> TotalBalances { get; set; } = new();
    public List<CurrencyTotal> TotalIncome { get; set; } = new();
    public List<CurrencyTotal> TotalExpenses { get; set; } = new();
    public List<MovementHistoryResponse> RecentMovements { get; set; } = new();
    public DateTime PeriodFrom { get; set; }
    public DateTime PeriodTo { get; set; }
}
