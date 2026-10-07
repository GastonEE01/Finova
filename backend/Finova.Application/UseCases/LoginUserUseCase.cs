using Finova.Application.DTOs;
using Finova.Application.Interfaces;

namespace Finova.Application.UseCases;

public class LoginUserUseCase
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtTokenGenerator _jwt;

    public LoginUserUseCase(IUserRepository users, IPasswordHasher hasher, IJwtTokenGenerator jwt)
    {
        _users = users;
        _hasher = hasher;
        _jwt = jwt;
    }

    public async Task<AuthResponse> ExecuteAsync(LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await _users.GetByEmailAsync(email);
        if (user is null)
            throw new UnauthorizedAccessException("Credenciales inválidas.");

        if (!_hasher.Verify(user, user.PasswordHash, request.Password))
            throw new UnauthorizedAccessException("Credenciales inválidas.");

        var (token, expiresAt) = _jwt.Generate(user);
        return new AuthResponse { Token = token, Email = user.Email, ExpiresAt = expiresAt };
    }
}
