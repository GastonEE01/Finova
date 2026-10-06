namespace Finova.Application.DTOs;

public class MonthTotalItem
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal Income { get; set; }
    public decimal Expense { get; set; }
}

public class TopCategoryItem
{
    public Guid? CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public decimal Percent { get; set; }
}

public class ExpenseEvolutionItem
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal Expense { get; set; }
}

public class ComparisonsResponse
{
    public string Currency { get; set; } = string.Empty;
    public MonthTotalItem CurrentMonth { get; set; } = new();
    public MonthTotalItem PreviousMonth { get; set; } = new();
    public decimal? IncomeVariationPct { get; set; }
    public decimal? ExpenseVariationPct { get; set; }
    public List<TopCategoryItem> TopCategories { get; set; } = new();
    public List<ExpenseEvolutionItem> ExpenseEvolution { get; set; } = new();
}
