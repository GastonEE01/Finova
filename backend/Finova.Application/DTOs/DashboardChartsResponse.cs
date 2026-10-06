namespace Finova.Application.DTOs;

public class CategoryExpenseItem
{
    public Guid? CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal Total { get; set; }
}

public class ExpensesByCategoryResponse
{
    public string Currency { get; set; } = string.Empty;
    public DateTime PeriodFrom { get; set; }
    public DateTime PeriodTo { get; set; }
    public List<CategoryExpenseItem> Items { get; set; } = new();
}

public class MonthlyTotalItem
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal Income { get; set; }
    public decimal Expense { get; set; }
}

public class IncomeVsExpensesResponse
{
    public string Currency { get; set; } = string.Empty;
    public List<MonthlyTotalItem> Months { get; set; } = new();
}

public class BalancePointItem
{
    public DateTime Date { get; set; }
    public decimal Balance { get; set; }
}

public class BalanceEvolutionResponse
{
    public string Currency { get; set; } = string.Empty;
    public DateTime PeriodFrom { get; set; }
    public DateTime PeriodTo { get; set; }
    public List<BalancePointItem> Points { get; set; } = new();
}
