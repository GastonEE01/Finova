namespace Finova.Application.DTOs;

public class ChatRequest
{
    public string Question { get; set; } = string.Empty;
}

public class ChatResponse
{
    public string Answer { get; set; } = string.Empty;
}

public class AssistantSnapshot
{
    public bool HasData { get; set; }
    public List<CurrencyTotal> TotalBalances { get; set; } = new();
    public List<CurrencyTotal> MonthIncome { get; set; } = new();
    public List<CurrencyTotal> MonthExpenses { get; set; } = new();
    public MonthTotalItem CurrentMonth { get; set; } = new();
    public MonthTotalItem PreviousMonth { get; set; } = new();
    public string MainCurrency { get; set; } = string.Empty;
    public decimal? ExpenseVariationPct { get; set; }
    public decimal? IncomeVariationPct { get; set; }
    public List<TopCategoryItem> TopCategories { get; set; } = new();
    public int MovementCount { get; set; }
    public string PeriodLabel { get; set; } = string.Empty;
}
