using Finova.Application.DTOs;
using Finova.Application.Interfaces;
using Finova.Domain.Entities;

namespace Finova.Application.UseCases;

public class RegisterUserUseCase
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtTokenGenerator _jwt;

    public RegisterUserUseCase(IUserRepository users, IPasswordHasher hasher, IJwtTokenGenerator jwt)
    {
        _users = users;
        _hasher = hasher;
        _jwt = jwt;
    }

    public async Task<AuthResponse> ExecuteAsync(RegisterRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (await _users.ExistsByEmailAsync(email))
            throw new InvalidOperationException("Ya existe un usuario con ese email.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            CreatedAt = DateTime.UtcNow
        };
        user.PasswordHash = _hasher.Hash(user, request.Password);

        await _users.AddAsync(user);

        var (token, expiresAt) = _jwt.Generate(user);
        return new AuthResponse { Token = token, Email = user.Email, ExpiresAt = expiresAt };
    }
}
