using System.Globalization;
using System.Text;
using System.Text.Json;
using Finova.Application.DTOs;
using Finova.Application.Exceptions;
using Finova.Application.Interfaces;
using Finova.Domain.Enums;

namespace Finova.Application.UseCases;

public class AskAssistantUseCase
{
    private readonly IAccountRepository _accounts;
    private readonly IMovementRepository _movements;
    private readonly IChatClient _chat;

    public AskAssistantUseCase(
        IAccountRepository accounts,
        IMovementRepository movements,
        IChatClient chat)
    {
        _accounts = accounts;
        _movements = movements;
        _chat = chat;
    }

    public async Task<ChatResponse> ExecuteAsync(Guid userId, string question)
    {
        var snapshot = await BuildSnapshotAsync(userId);
        var snapshotJson = JsonSerializer.Serialize(snapshot, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var systemPrompt = new StringBuilder()
            .AppendLine("Sos el asistente financiero de Finova. Respondé en español, breve y claro.")
            .AppendLine("ÚNICA fuente de verdad (JSON snapshot calculado por el backend): " + snapshotJson)
            .AppendLine("REGLAS: (1) Usá SOLO cifras del snapshot, no inventes ni estimes montos, saldos ni categorías.")
            .AppendLine("(2) Si el dato no está en el snapshot o hasData es false, decí \"Aún no tenés datos registrados\" o \"No tengo ese dato\" sin dar cifras.")
            .AppendLine("(3) Los consejos (por ejemplo cómo reducir gastos) deben basarse solo en las categorías reales de topCategories.")
            .AppendLine("(4) Si la pregunta no es de finanzas personales, derivala a finanzas o decí que no podés ayudar con eso.")
            .AppendLine("(5) Nunca menciones otros usuarios ni datos ajenos. Indicá siempre la moneda de cada cifra.")
            .ToString();

        try
        {
            var answer = await _chat.GetAnswerAsync(systemPrompt, question);
            if (string.IsNullOrWhiteSpace(answer))
                throw new AssistantUnavailableException("Respuesta vacía");
            return new ChatResponse { Answer = answer.Trim() };
        }
        catch (AssistantUnavailableException)
        {
            throw;
        }
        catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException)
        {
            throw new AssistantUnavailableException("Ollama no disponible", ex);
        }
    }

    private async Task<AssistantSnapshot> BuildSnapshotAsync(Guid userId)
    {
        var now = DateTime.UtcNow;
        var prevStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-1);

        var accounts = await _accounts.ListByUserWithMovementsAsync(userId);
        var totalBalances = accounts
            .GroupBy(a => a.Currency)
            .Select(g => new CurrencyTotal
            {
                Currency = g.Key,
                Amount = g.Sum(a => a.Movements.Sum(m => m.Type == MovementType.Income ? m.Amount : -m.Amount))
            })
            .ToList();

        var movements = await _movements.ListByUserWithDetailsAsync(userId);

        decimal SumByMonth(MovementType type, int year, int month, string? currency = null) =>
            movements.Where(m => m.Type == type && m.Date.Year == year && m.Date.Month == month
                    && (currency == null || m.Account.Currency == currency))
                .Sum(m => m.Amount);

        var monthIncome = movements
            .Where(m => m.Type == MovementType.Income && m.Date.Year == now.Year && m.Date.Month == now.Month)
            .GroupBy(m => m.Account.Currency)
            .Select(g => new CurrencyTotal { Currency = g.Key, Amount = g.Sum(x => x.Amount) })
            .ToList();

        var monthExpenses = movements
            .Where(m => m.Type == MovementType.Expense && m.Date.Year == now.Year && m.Date.Month == now.Month)
            .GroupBy(m => m.Account.Currency)
            .Select(g => new CurrencyTotal { Currency = g.Key, Amount = g.Sum(x => x.Amount) })
            .ToList();

        var mainCurrency = totalBalances
            .OrderByDescending(t => t.Amount)
            .Select(t => t.Currency)
            .FirstOrDefault()
            ?? monthIncome.Concat(monthExpenses)
                .OrderByDescending(t => t.Amount)
                .Select(t => t.Currency)
                .FirstOrDefault()
            ?? string.Empty;

        var current = new MonthTotalItem
        {
            Year = now.Year,
            Month = now.Month,
            Income = string.IsNullOrEmpty(mainCurrency) ? 0 : SumByMonth(MovementType.Income, now.Year, now.Month, mainCurrency),
            Expense = string.IsNullOrEmpty(mainCurrency) ? 0 : SumByMonth(MovementType.Expense, now.Year, now.Month, mainCurrency)
        };
        var previous = new MonthTotalItem
        {
            Year = prevStart.Year,
            Month = prevStart.Month,
            Income = string.IsNullOrEmpty(mainCurrency) ? 0 : SumByMonth(MovementType.Income, prevStart.Year, prevStart.Month, mainCurrency),
            Expense = string.IsNullOrEmpty(mainCurrency) ? 0 : SumByMonth(MovementType.Expense, prevStart.Year, prevStart.Month, mainCurrency)
        };

        var currentExpenses = string.IsNullOrEmpty(mainCurrency)
            ? new List<(Guid? CategoryId, string? CategoryName, decimal Amount)>()
            : movements
                .Where(m => m.Type == MovementType.Expense && m.Date.Year == now.Year
                    && m.Date.Month == now.Month && m.Account.Currency == mainCurrency)
                .Select(m => (CategoryId: m.CategoryId, CategoryName: m.Category?.Name, Amount: m.Amount))
                .ToList();

        var monthTotal = currentExpenses.Sum(m => m.Amount);
        var top = currentExpenses
            .GroupBy(m => new { m.CategoryId, Name = m.CategoryName ?? "Sin categoría" })
            .Select(g => new TopCategoryItem
            {
                CategoryId = g.Key.CategoryId,
                CategoryName = g.Key.Name,
                Total = g.Sum(x => x.Amount),
                Percent = monthTotal == 0 ? 0 : Math.Round(g.Sum(x => x.Amount) / monthTotal * 100, 2)
            })
            .OrderByDescending(x => x.Total)
            .Take(5)
            .ToList();

        var periodLabel = now.ToString("MMMM yyyy", new CultureInfo("es-ES"));

        return new AssistantSnapshot
        {
            HasData = movements.Count > 0,
            TotalBalances = totalBalances,
            MonthIncome = monthIncome,
            MonthExpenses = monthExpenses,
            CurrentMonth = current,
            PreviousMonth = previous,
            MainCurrency = mainCurrency,
            ExpenseVariationPct = DashboardCalculations.Variation(current.Expense, previous.Expense),
            IncomeVariationPct = DashboardCalculations.Variation(current.Income, previous.Income),
            TopCategories = top,
            MovementCount = movements.Count,
            PeriodLabel = periodLabel
        };
    }
}
