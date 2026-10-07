using Finova.Application.DTOs;
using Finova.Application.Interfaces;
using Finova.Application.UseCases;
using Finova.Domain.Entities;
using FluentAssertions;
using Moq;

namespace Finova.Tests;

public class AuthUseCasesTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<IJwtTokenGenerator> _jwt = new();

    private RegisterUserUseCase Register() => new(_users.Object, _hasher.Object, _jwt.Object);
    private LoginUserUseCase Login() => new(_users.Object, _hasher.Object, _jwt.Object);

    [Fact]
    public async Task Register_CreaUsuario_YDevuelveToken()
    {
        _users.Setup(r => r.ExistsByEmailAsync("a@b.com")).ReturnsAsync(false);
        _jwt.Setup(j => j.Generate(It.IsAny<User>())).Returns(("tok", DateTime.UtcNow.AddMinutes(60)));

        var result = await Register().ExecuteAsync(new RegisterRequest { Email = "A@B.com", Password = "secreto123" });

        result.Token.Should().Be("tok");
        result.Email.Should().Be("a@b.com");
        _users.Verify(r => r.AddAsync(It.Is<User>(u => u.Email == "a@b.com")), Times.Once);
    }

    [Fact]
    public async Task Register_EmailDuplicado_LanzaInvalidOperation()
    {
        _users.Setup(r => r.ExistsByEmailAsync("a@b.com")).ReturnsAsync(true);

        var act = () => Register().ExecuteAsync(new RegisterRequest { Email = "a@b.com", Password = "secreto123" });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Ya existe un usuario con ese email.");
    }

    [Fact]
    public async Task Register_NormalizaEmail_BuscaEnMinusculas()
    {
        _users.Setup(r => r.ExistsByEmailAsync("a@b.com")).ReturnsAsync(false);
        _jwt.Setup(j => j.Generate(It.IsAny<User>())).Returns(("tok", DateTime.UtcNow));

        await Register().ExecuteAsync(new RegisterRequest { Email = "  A@B.COM ", Password = "secreto123" });

        _users.Verify(r => r.ExistsByEmailAsync("a@b.com"), Times.Once);
    }

    [Fact]
    public async Task Login_CredencialesValidas_DevuelveToken()
    {
        var user = new User { Id = Guid.NewGuid(), Email = "a@b.com", PasswordHash = "hash" };
        _users.Setup(r => r.GetByEmailAsync("a@b.com")).ReturnsAsync(user);
        _hasher.Setup(h => h.Verify(user, "hash", "secreto123")).Returns(true);
        _jwt.Setup(j => j.Generate(user)).Returns(("tok", DateTime.UtcNow.AddMinutes(60)));

        var result = await Login().ExecuteAsync(new LoginRequest { Email = "a@b.com", Password = "secreto123" });

        result.Token.Should().Be("tok");
    }

    [Fact]
    public async Task Login_UsuarioInexistente_LanzaUnauthorized()
    {
        _users.Setup(r => r.GetByEmailAsync("x@b.com")).ReturnsAsync((User?)null);

        var act = () => Login().ExecuteAsync(new LoginRequest { Email = "x@b.com", Password = "zzz" });

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Login_PasswordIncorrecto_LanzaUnauthorized()
    {
        var user = new User { Id = Guid.NewGuid(), Email = "a@b.com", PasswordHash = "hash" };
        _users.Setup(r => r.GetByEmailAsync("a@b.com")).ReturnsAsync(user);
        _hasher.Setup(h => h.Verify(user, "hash", "mal")).Returns(false);

        var act = () => Login().ExecuteAsync(new LoginRequest { Email = "a@b.com", Password = "mal" });

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
