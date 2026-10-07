using Finova.Application.Interfaces;
using Finova.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Finova.Infrastructure.Repositories;

public class PasswordHasherAdapter : IPasswordHasher
{
    private readonly PasswordHasher<User> _hasher = new();

    public string Hash(User user, string password) => _hasher.HashPassword(user, password);

    public bool Verify(User user, string hash, string password) =>
        _hasher.VerifyHashedPassword(user, hash, password) != PasswordVerificationResult.Failed;
}
