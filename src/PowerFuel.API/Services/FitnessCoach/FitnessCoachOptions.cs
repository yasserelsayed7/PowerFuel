namespace PowerFuel.API.Services.FitnessCoach;

public sealed class FitnessCoachOptions
{
    public const string SectionName = "FitnessCoach";

    /// <summary>Base URL of the fitness-coach FastAPI (e.g. http://localhost:8000).</summary>
    public string BaseUrl { get; set; } = "http://localhost:8000";

    /// <summary>Sent on every request as header X-API-Key (matches FITCOACH_API_KEY in FastAPI).</summary>
    public string ApiKey { get; set; } = string.Empty;
}
