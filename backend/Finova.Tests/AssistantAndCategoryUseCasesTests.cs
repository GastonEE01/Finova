using Finova.Application.Exceptions;
using Finova.Application.Interfaces;
using Finova.Application.UseCases;
using Finova.Domain.Entities;
using Finova.Domain.Enums;
using FluentAssertions;
using Moq;

namespace Finova.Tests;

public class AssistantAndCategoryUseCasesTests
{
    private readonly Mock<IAccountRepository> _accounts = new();
    private readonly Mock<IMovementRepository> _movements = new();
    private readonly Mock<IChatClient> _chat = new();
    private readonly Mock<ICategoryRepository> _categories = new();
    private readonly Guid _userId = Guid.NewGuid();

    [Fact]
    public async Task Ask_RespondeConTextoDelChat()
    {
        _accounts.Setup(r => r.ListByUserWithMovementsAsync(_userId)).ReturnsAsync(new List<Account>());
        _movements.Setup(r => r.ListByUserWithDetailsAsync(_userId)).ReturnsAsync(new List<Movement>());
        _chat.Setup(c => c.GetAnswerAsync(It.IsAny<string>(), "¿Cómo voy?"))
            .ReturnsAsync("Vas bien.");

        var result = await new AskAssistantUseCase(_accounts.Object, _movements.Object, _chat.Object)
            .ExecuteAsync(_userId, "¿Cómo voy?");

        result.Answer.Should().Be("Vas bien.");
    }

    [Fact]
    public async Task Ask_RespuestaVacia_LanzaNoDisponible()
    {
        _accounts.Setup(r => r.ListByUserWithMovementsAsync(_userId)).ReturnsAsync(new List<Account>());
        _movements.Setup(r => r.ListByUserWithDetailsAsync(_userId)).ReturnsAsync(new List<Movement>());
        _chat.Setup(c => c.GetAnswerAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("  ");

        var act = () => new AskAssistantUseCase(_accounts.Object, _movements.Object, _chat.Object)
            .ExecuteAsync(_userId, "hola");

        await act.Should().ThrowAsync<AssistantUnavailableException>();
    }

    [Fact]
    public async Task Ask_ChatCaido_LanzaNoDisponible()
    {
        _accounts.Setup(r => r.ListByUserWithMovementsAsync(_userId)).ReturnsAsync(new List<Account>());
        _movements.Setup(r => r.ListByUserWithDetailsAsync(_userId)).ReturnsAsync(new List<Movement>());
        _chat.Setup(c => c.GetAnswerAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new HttpRequestException("down"));

        var act = () => new AskAssistantUseCase(_accounts.Object, _movements.Object, _chat.Object)
            .ExecuteAsync(_userId, "hola");

        await act.Should().ThrowAsync<AssistantUnavailableException>();
    }

    [Fact]
    public async Task Categories_ListaSoloAccesibles()
    {
        _categories.Setup(r => r.ListAccessibleAsync(_userId, MovementType.Expense)).ReturnsAsync(new List<Category>
        {
            new() { Id = Guid.NewGuid(), Name = "Comida", Type = MovementType.Expense }
        });

        var result = await new ListCategoriesUseCase(_categories.Object)
            .ExecuteAsync(_userId, MovementType.Expense);

        result.Should().ContainSingle().Which.Name.Should().Be("Comida");
        _categories.Verify(r => r.ListAccessibleAsync(_userId, MovementType.Expense), Times.Once);
    }

    [Fact]
    public async Task Categories_SinFiltro_PideTodo()
    {
        _categories.Setup(r => r.ListAccessibleAsync(_userId, null)).ReturnsAsync(new List<Category>());

        var result = await new ListCategoriesUseCase(_categories.Object).ExecuteAsync(_userId, null);

        result.Should().BeEmpty();
        _categories.Verify(r => r.ListAccessibleAsync(_userId, null), Times.Once);
    }

    [Fact]
    public async Task Categories_OtroUsuario_NoMezclaLlamadas()
    {
        var other = Guid.NewGuid();
        _categories.Setup(r => r.ListAccessibleAsync(other, null)).ReturnsAsync(new List<Category>());

        await new ListCategoriesUseCase(_categories.Object).ExecuteAsync(other, null);

        _categories.Verify(r => r.ListAccessibleAsync(other, null), Times.Once);
        _categories.Verify(r => r.ListAccessibleAsync(_userId, null), Times.Never);
    }
}
