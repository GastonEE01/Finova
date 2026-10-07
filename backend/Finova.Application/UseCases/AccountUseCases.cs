using Finova.Application.DTOs;
using Finova.Application.Interfaces;
using Finova.Domain.Entities;
using Finova.Domain.Enums;

namespace Finova.Application.UseCases;

public static class AccountBalance
{
    public static decimal Of(Account account) =>
        account.Movements.Sum(m => m.Type == MovementType.Income ? m.Amount : -m.Amount);

    public static AccountResponse ToResponse(Account account) => new()
    {
        Id = account.Id,
        Name = account.Name,
        Currency = account.Currency,
        Balance = Of(account)
    };
}

public class CreateAccountUseCase
{
    private readonly IAccountRepository _accounts;

    public CreateAccountUseCase(IAccountRepository accounts)
    {
        _accounts = accounts;
    }

    public async Task<AccountResponse> ExecuteAsync(Guid userId, CreateAccountRequest request)
    {
        var account = new Account
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = request.Name.Trim(),
            Currency = request.Currency.Trim().ToUpperInvariant()
        };

        await _accounts.AddAsync(account);

        return new AccountResponse
        {
            Id = account.Id,
            Name = account.Name,
            Currency = account.Currency,
            Balance = 0
        };
    }
}

public class ListAccountsUseCase
{
    private readonly IAccountRepository _accounts;

    public ListAccountsUseCase(IAccountRepository accounts)
    {
        _accounts = accounts;
    }

    public async Task<List<AccountResponse>> ExecuteAsync(Guid userId)
    {
        var accounts = await _accounts.ListByUserWithMovementsAsync(userId);
        return accounts.Select(AccountBalance.ToResponse).ToList();
    }
}

public class GetAccountUseCase
{
    private readonly IAccountRepository _accounts;

    public GetAccountUseCase(IAccountRepository accounts)
    {
        _accounts = accounts;
    }

    public async Task<AccountResponse?> ExecuteAsync(Guid userId, Guid accountId)
    {
        var account = await _accounts.GetByUserWithMovementsAsync(userId, accountId);
        return account is null ? null : AccountBalance.ToResponse(account);
    }
}
