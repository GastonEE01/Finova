using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Finova.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AuthController : ControllerBase
{
    private readonly Finova.Application.UseCases.RegisterUserUseCase _register;
    private readonly Finova.Application.UseCases.LoginUserUseCase _login;

    public AuthController(
        Finova.Application.UseCases.RegisterUserUseCase register,
        Finova.Application.UseCases.LoginUserUseCase login)
    {
        _register = register;
        _login = login;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(Finova.Application.DTOs.RegisterRequest request)
    {
        try
        {
            var response = await _register.ExecuteAsync(request);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { mensaje = ex.Message });
        }
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(Finova.Application.DTOs.LoginRequest request)
    {
        try
        {
            var response = await _login.ExecuteAsync(request);
            return Ok(response);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { mensaje = "Credenciales inválidas." });
        }
    }

    [HttpGet("me")]
    public IActionResult Me()
    {
        var email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email");
        return Ok(new { email });
    }
}
