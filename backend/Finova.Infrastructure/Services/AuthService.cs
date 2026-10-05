using Finova.Application.DTOs;
using Finova.Application.Interfaces;
using Finova.Domain.Entities;
using Finova.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Finova.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly FinovaDbContext _context;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly PasswordHasher<User> _passwordHasher = new();

    public AuthService(FinovaDbContext context, IJwtTokenGenerator jwtTokenGenerator)
    {
        _context = context;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var exists = await _context.Users.AnyAsync(u => u.Email == email);
        if (exists)
            throw new InvalidOperationException("Ya existe un usuario con ese email.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            CreatedAt = DateTime.UtcNow
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var (token, expiresAt) = _jwtTokenGenerator.Generate(user);
        return new AuthResponse { Token = token, Email = user.Email, ExpiresAt = expiresAt };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user is null)
            throw new UnauthorizedAccessException("Credenciales inválidas.");

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
            throw new UnauthorizedAccessException("Credenciales inválidas.");

        var (token, expiresAt) = _jwtTokenGenerator.Generate(user);
        return new AuthResponse { Token = token, Email = user.Email, ExpiresAt = expiresAt };
    }
}
