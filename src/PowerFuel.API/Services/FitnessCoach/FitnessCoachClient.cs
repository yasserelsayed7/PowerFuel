using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace PowerFuel.API.Services.FitnessCoach;

public sealed class FitnessCoachClient(HttpClient httpClient) : IFitnessCoachClient
{
    private static readonly MediaTypeHeaderValue JsonMedia = MediaTypeHeaderValue.Parse("application/json; charset=utf-8");

    public async Task<JsonElement> StartSessionAsync(JsonElement requestBody, CancellationToken cancellationToken = default)
    {
        var text = requestBody.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null
            ? "{}"
            : requestBody.GetRawText();
        return await PostJsonAsync("start-session", text, cancellationToken);
    }

    public async Task<JsonElement> AnalyzeFrameAsync(JsonElement requestBody, CancellationToken cancellationToken = default)
    {
        return await PostJsonAsync("analyze-frame", requestBody.GetRawText(), cancellationToken);
    }

    public async Task<JsonElement> EndSessionAsync(JsonElement requestBody, CancellationToken cancellationToken = default)
    {
        return await PostJsonAsync("end-session", requestBody.GetRawText(), cancellationToken);
    }

    public async Task<JsonElement> GetSessionSummaryAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentException("Session id is required.", nameof(sessionId));

        // session_id in path — encode for safety
        var path = "session-summary/" + Uri.EscapeDataString(sessionId);
        using var response = await httpClient.GetAsync(path, cancellationToken);
        return await ReadJsonOrThrowAsync(response, cancellationToken);
    }

    private async Task<JsonElement> PostJsonAsync(string relativePath, string json, CancellationToken cancellationToken)
    {
        using var content = new StringContent(json, Encoding.UTF8);
        content.Headers.ContentType = JsonMedia;
        using var response = await httpClient.PostAsync(relativePath, content, cancellationToken);
        return await ReadJsonOrThrowAsync(response, cancellationToken);
    }

    private static async Task<JsonElement> ReadJsonOrThrowAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            if (string.IsNullOrWhiteSpace(body))
            {
                using var empty = JsonDocument.Parse("{}");
                return empty.RootElement.Clone();
            }
            using var doc = JsonDocument.Parse(body);
            return doc.RootElement.Clone();
        }

        throw new HttpRequestException(
            $"Fitness coach service returned {(int)response.StatusCode} {response.ReasonPhrase}. Body: {body}",
            null,
            response.StatusCode);
    }
}
