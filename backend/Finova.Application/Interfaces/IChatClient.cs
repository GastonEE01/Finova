namespace Finova.Application.Interfaces;

public interface IChatClient
{
    Task<string> GetAnswerAsync(string systemPrompt, string question);
}
