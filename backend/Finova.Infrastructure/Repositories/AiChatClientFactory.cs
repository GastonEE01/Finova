using Finova.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Finova.Infrastructure.Repositories;

public class AiChatClientFactory : IChatClient
{
    private readonly IChatClient _active;

    public AiChatClientFactory(
        IConfiguration config,
        OllamaChatClient ollama,
        GeminiChatClient gemini)
    {
        var provider = config["AiProvider"];
        _active = string.Equals(provider, "Gemini", StringComparison.OrdinalIgnoreCase)
            ? gemini
            : ollama;
    }

    public Task<string> GetAnswerAsync(string systemPrompt, string question)
        => _active.GetAnswerAsync(systemPrompt, question);
}
