using System.Text;
using System.Text.Json;
using Finova.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Finova.Infrastructure.Repositories;

public class OllamaChatClient : IChatClient
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;

    public OllamaChatClient(HttpClient http, IConfiguration config)
    {
        _http = http;
        _config = config;
    }

    public async Task<string> GetAnswerAsync(string systemPrompt, string question)
    {
        var baseUrl = (_config["Ollama:BaseUrl"] ?? "http://localhost:11434").TrimEnd('/');
        var model = _config["Ollama:Model"] ?? "llama3.1:8b";

        var payload = new
        {
            model,
            messages = new object[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = question }
            },
            stream = false,
            options = new { temperature = 0.2 }
        };

        using var content = new StringContent(
            JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        using var response = await _http.PostAsync($"{baseUrl}/api/chat", content);
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Ollama devolvió {(int)response.StatusCode}");

        using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        var answer = doc.RootElement.TryGetProperty("message", out var msg)
            && msg.TryGetProperty("content", out var text)
            ? text.GetString() ?? string.Empty
            : string.Empty;

        return answer.Trim();
    }
}
