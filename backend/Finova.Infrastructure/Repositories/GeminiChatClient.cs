using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Finova.Application.Exceptions;
using Finova.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Finova.Infrastructure.Repositories;

public class GeminiChatClient(HttpClient http, IConfiguration config) : IChatClient
{
    private readonly HttpClient _http = http;
    private readonly IConfiguration _config = config;

    public async Task<string> GetAnswerAsync(string systemPrompt, string question)
    {
        var apiKey = _config["Gemini:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new AssistantUnavailableException("Proveedor IA no disponible");

        var model = _config["Gemini:Model"];
        if (string.IsNullOrWhiteSpace(model))
            model = "gemini-3.8-flash";

        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";
        var payload = new GeminiRequest(
            new InstructionPart([new TextPart(systemPrompt)]),
            [new ContentItem("user", [new TextPart(question)])],
            new GenerationConfig(0.2));

        using var response = await _http.PostAsJsonAsync(url, payload);
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Gemini devolvió {(int)response.StatusCode}");

        using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        if (!doc.RootElement.TryGetProperty("candidates", out var candidates) || candidates.GetArrayLength() == 0)
            return string.Empty;

        var texts = new List<string>();
        foreach (var candidate in candidates.EnumerateArray())
        {
            if (!candidate.TryGetProperty("content", out var content)) continue;
            if (!content.TryGetProperty("parts", out var parts)) continue;
            foreach (var part in parts.EnumerateArray())
            {
                if (part.TryGetProperty("text", out var text))
                    texts.Add(text.GetString() ?? string.Empty);
            }
        }

        return string.Concat(texts).Trim();
    }

    private sealed record GeminiRequest(
        [property: JsonPropertyName("system_instruction")] InstructionPart SystemInstruction,
        [property: JsonPropertyName("contents")] List<ContentItem> Contents,
        [property: JsonPropertyName("generationConfig")] GenerationConfig GenerationConfig);

    private sealed record InstructionPart(List<TextPart> Parts);

    private sealed record ContentItem(string Role, List<TextPart> Parts);

    private sealed record TextPart(string Text);

    private sealed record GenerationConfig(double Temperature);
}
