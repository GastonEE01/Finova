using System.ComponentModel.DataAnnotations;

namespace Finova.Application.DTOs;

public class CreateAccountRequest
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(3, MinimumLength = 3)]
    [RegularExpression("^[A-Za-z]{3}$", ErrorMessage = "La moneda debe ser un código ISO de 3 letras.")]
    public string Currency { get; set; } = "ARS";
}
