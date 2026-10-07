using System.Net;
using System.Text;
using Finova.Application.Exceptions;
using Finova.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;

namespace Finova.Tests;

public class GeminiChatClientTests
{
    private static IConfiguration ConfigWith(Dictionary<string, string?> values)
    {
        var mock = new Mock<IConfiguration>();
        mock.Setup(c => c[It.IsAny<string>()])
            .Returns((string key) => values.TryGetValue(key, out var v) ? v : null);
        return mock.Object;
    }

    private static HttpClient HttpWith(HttpResponseMessage response)
    {
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
        return new HttpClient(handler.Object);
    }

    private static HttpClient HttpThatMustNotBeCalled()
    {
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Throws(new InvalidOperationException("No debe llamar a red sin key"));
        return new HttpClient(handler.Object);
    }

    [Fact]
    public async Task ParseaYConcatenaParts()
    {
        var json = """{"candidates":[{"content":{"parts":[{"text":"Hola "},{"text":"mundo"}]}}]}""";
        var client = new GeminiChatClient(
            HttpWith(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            }),
            ConfigWith(new() { ["Gemini:ApiKey"] = "fake-key", ["Gemini:Model"] = "gemini-3.8-flash" }));

        var result = await client.GetAnswerAsync("sys", "pregunta");

        result.Should().Be("Hola mundo");
    }

    [Fact]
    public async Task RespuestaSinCandidates_RetornaVacio()
    {
        var client = new GeminiChatClient(
            HttpWith(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}", Encoding.UTF8, "application/json")
            }),
            ConfigWith(new() { ["Gemini:ApiKey"] = "fake-key" }));

        var result = await client.GetAnswerAsync("sys", "pregunta");

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Error429_LanzaHttpRequestException()
    {
        var client = new GeminiChatClient(
            HttpWith(new HttpResponseMessage((HttpStatusCode)429)),
            ConfigWith(new() { ["Gemini:ApiKey"] = "fake-key" }));

        var act = () => client.GetAnswerAsync("sys", "pregunta");

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task SinApiKey_LanzaNoDisponibleSinLlamarRed()
    {
        var client = new GeminiChatClient(
            HttpThatMustNotBeCalled(),
            ConfigWith(new() { ["Gemini:Model"] = "gemini-3.8-flash" }));

        var act = () => client.GetAnswerAsync("sys", "pregunta");

        await act.Should().ThrowAsync<AssistantUnavailableException>();
    }

    [Theory]
    [InlineData("Gemini", true)]
    [InlineData("gemini", true)]
    [InlineData("Ollama", false)]
    [InlineData(null, false)]
    [InlineData("Invalido", false)]
    public void Factory_DelegaSegunAiProvider(string? provider, bool expectGemini)
    {
        var values = new Dictionary<string, string?> { ["Gemini:ApiKey"] = "fake-key" };
        if (provider is not null) values["AiProvider"] = provider;
        var config = ConfigWith(values);

        var ollama = new OllamaChatClient(HttpThatMustNotBeCalled(), config);
        var gemini = new GeminiChatClient(HttpThatMustNotBeCalled(), config);
        var factory = new AiChatClientFactory(config, ollama, gemini);

        var field = typeof(AiChatClientFactory).GetField(
            "_active", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        var active = field.GetValue(factory);

        if (expectGemini) active.Should().BeSameAs(gemini);
        else active.Should().BeSameAs(ollama);
    }
}
