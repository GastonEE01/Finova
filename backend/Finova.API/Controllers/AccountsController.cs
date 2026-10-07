using Finova.Application.DTOs;
using Finova.Application.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Finova.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AccountsController : ControllerBase
{
    private readonly CreateAccountUseCase _create;
    private readonly ListAccountsUseCase _list;
    private readonly GetAccountUseCase _get;

    public AccountsController(
        CreateAccountUseCase create,
        ListAccountsUseCase list,
        GetAccountUseCase get)
    {
        _create = create;
        _list = list;
        _get = get;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var accounts = await _list.ExecuteAsync(GetUserId());
        return Ok(accounts);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var account = await _get.ExecuteAsync(GetUserId(), id);
        return account is null ? NotFound() : Ok(account);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateAccountRequest request)
    {
        var account = await _create.ExecuteAsync(GetUserId(), request);
        return CreatedAtAction(nameof(GetById), new { id = account.Id }, account);
    }

    private Guid GetUserId()
    {
        var sub = User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(sub!);
    }
}
