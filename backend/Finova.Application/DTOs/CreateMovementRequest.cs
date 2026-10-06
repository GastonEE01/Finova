using System.ComponentModel.DataAnnotations;
using Finova.Domain.Enums;

namespace Finova.Application.DTOs;

public class CreateMovementRequest
{
    [Required]
    public Guid AccountId { get; set; }

    public Guid? CategoryId { get; set; }

    [Required]
    public MovementType Type { get; set; }

    [Required, Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }
}
