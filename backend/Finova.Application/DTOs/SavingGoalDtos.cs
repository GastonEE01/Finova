using System.ComponentModel.DataAnnotations;

namespace Finova.Application.DTOs;

public class CreateSavingGoalRequest
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, Range(0.01, double.MaxValue)]
    public decimal TargetAmount { get; set; }

    [Required]
    public DateTime TargetDate { get; set; }
}

public class UpdateSavingGoalRequest
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, Range(0.01, double.MaxValue)]
    public decimal TargetAmount { get; set; }

    [Required]
    public DateTime TargetDate { get; set; }
}

public class SavingGoalResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal TargetAmount { get; set; }
    public decimal Progress { get; set; }
    public decimal Remaining { get; set; }
    public decimal Percent { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime TargetDate { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AddContributionRequest
{
    [Required]
    public Guid AccountId { get; set; }

    [Required]
    public Guid CategoryId { get; set; }

    [Required, Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }
}

public class ContributionResponse : SavingGoalResponse
{
    public Guid MovementId { get; set; }
}
