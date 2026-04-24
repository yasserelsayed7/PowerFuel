using System.Text.Json;

namespace PowerFuel.API.Services.FitnessCoach;

public interface IFitnessCoachClient
{
    Task<JsonElement> StartSessionAsync(JsonElement requestBody, CancellationToken cancellationToken = default);
    Task<JsonElement> AnalyzeFrameAsync(JsonElement requestBody, CancellationToken cancellationToken = default);
    Task<JsonElement> EndSessionAsync(JsonElement requestBody, CancellationToken cancellationToken = default);
    Task<JsonElement> GetSessionSummaryAsync(string sessionId, CancellationToken cancellationToken = default);
}
