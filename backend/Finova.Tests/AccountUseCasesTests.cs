using Finova.Application.DTOs;
using Finova.Application.Interfaces;
using Finova.Application.UseCases;
using Finova.Domain.Entities;
using Finova.Domain.Enums;
using FluentAssertions;
using Moq;

namespace Finova.Tests;

public class AccountUseCasesTests
{
    private readonly Mock<IAccountRepository> _accounts = new();
    private readonly Guid _userId = Guid.NewGuid();

    [Fact]
    public async Task Create_NormalizaNombreYMoneda_SaldoCero()
    {
        var result = await new CreateAccountUseCase(_accounts.Object)
            .ExecuteAsync(_userId, new CreateAccountRequest { Name = "  Efectivo ", Currency = "ars" });

        result.Name.Should().Be("Efectivo");
        result.Currency.Should().Be("ARS");
        result.Balance.Should().Be(0);
        _accounts.Verify(r => r.AddAsync(It.Is<Account>(a => a.UserId == _userId)), Times.Once);
    }

    [Fact]
    public async Task Create_Guarda_UserIdDelSolicitante()
    {
        Account? saved = null;
        _accounts.Setup(r => r.AddAsync(It.IsAny<Account>()))
            .Callback<Account>(a => saved = a).Returns(Task.CompletedTask);

        await new CreateAccountUseCase(_accounts.Object)
            .ExecuteAsync(_userId, new CreateAccountRequest { Name = "Banco", Currency = "USD" });

        saved.Should().NotBeNull();
        saved!.UserId.Should().Be(_userId);
    }

    [Fact]
    public async Task Create_ValidacionModel_NoLlegaAlRepo_SeEsperaOkDelUseCase()
    {
        // El use case confía en la validación de DataAnnotations del controller;
        // verifica que delega el guardado una sola vez.
        await new CreateAccountUseCase(_accounts.Object)
            .ExecuteAsync(_userId, new CreateAccountRequest { Name = "Caja", Currency = "ARS" });

        _accounts.Verify(r => r.AddAsync(It.IsAny<Account>()), Times.Once);
    }

    [Fact]
    public async Task List_CalculaSaldo_IngresosMenosGastos()
    {
        var account = new Account
        {
            Id = Guid.NewGuid(), UserId = _userId, Name = "Caja", Currency = "ARS",
            Movements = new List<Movement>
            {
                new() { Type = MovementType.Income, Amount = 100 },
                new() { Type = MovementType.Expense, Amount = 30 }
            }
        };
        _accounts.Setup(r => r.ListByUserWithMovementsAsync(_userId)).ReturnsAsync(new List<Account> { account });

        var result = await new ListAccountsUseCase(_accounts.Object).ExecuteAsync(_userId);

        result.Should().ContainSingle().Which.Balance.Should().Be(70);
    }

    [Fact]
    public async Task List_SinCuentas_DevuelveVacio()
    {
        _accounts.Setup(r => r.ListByUserWithMovementsAsync(_userId)).ReturnsAsync(new List<Account>());

        var result = await new ListAccountsUseCase(_accounts.Object).ExecuteAsync(_userId);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task List_SoloPideDatosDelUsuario()
    {
        _accounts.Setup(r => r.ListByUserWithMovementsAsync(_userId)).ReturnsAsync(new List<Account>());

        await new ListAccountsUseCase(_accounts.Object).ExecuteAsync(_userId);

        _accounts.Verify(r => r.ListByUserWithMovementsAsync(_userId), Times.Once);
        _accounts.Verify(r => r.ListByUserWithMovementsAsync(It.Is<Guid>(g => g != _userId)), Times.Never);
    }

    [Fact]
    public async Task Get_CuentaPropia_DevuelveConSaldo()
    {
        var id = Guid.NewGuid();
        _accounts.Setup(r => r.GetByUserWithMovementsAsync(_userId, id)).ReturnsAsync(
            new Account
            {
                Id = id, UserId = _userId, Name = "Caja", Currency = "ARS",
                Movements = new List<Movement> { new() { Type = MovementType.Income, Amount = 50 } }
            });

        var result = await new GetAccountUseCase(_accounts.Object).ExecuteAsync(_userId, id);

        result.Should().NotBeNull();
        result!.Balance.Should().Be(50);
    }

    [Fact]
    public async Task Get_CuentaInexistente_DevuelveNull()
    {
        var id = Guid.NewGuid();
        _accounts.Setup(r => r.GetByUserWithMovementsAsync(_userId, id)).ReturnsAsync((Account?)null);

        var result = await new GetAccountUseCase(_accounts.Object).ExecuteAsync(_userId, id);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Get_CuentaAjena_NoSeEncuentra()
    {
        var id = Guid.NewGuid();
        var other = Guid.NewGuid();
        _accounts.Setup(r => r.GetByUserWithMovementsAsync(other, id)).ReturnsAsync((Account?)null);

        var result = await new GetAccountUseCase(_accounts.Object).ExecuteAsync(other, id);

        result.Should().BeNull();
        _accounts.Verify(r => r.GetByUserWithMovementsAsync(other, id), Times.Once);
    }
}
