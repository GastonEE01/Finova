using System.ComponentModel.DataAnnotations;

namespace Finova.Application.DTOs;

public class CreateBudgetRequest
{
    [Required]
    public Guid CategoryId { get; set; }

    [Required, Range(2000, 2100)]
    public int Year { get; set; }

    [Required, Range(1, 12)]
    public int Month { get; set; }

    [Required, Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    [Required, MaxLength(3)]
    public string Currency { get; set; } = string.Empty;
}

public class UpdateBudgetRequest
{
    [Required, Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }
}

public class BudgetResponse
{
    public Guid Id { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public decimal Spent { get; set; }
    public decimal Remaining { get; set; }
    public decimal Percent { get; set; }
    public string Status { get; set; } = string.Empty;
}
