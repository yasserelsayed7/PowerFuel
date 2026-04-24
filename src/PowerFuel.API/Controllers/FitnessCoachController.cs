using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using PowerFuel.API.Services.FitnessCoach;

namespace PowerFuel.API.Controllers;

[ApiController]
[Route("api/fitness-coach")]
public class FitnessCoachController : ControllerBase
{
    private readonly IFitnessCoachClient _client;

    public FitnessCoachController(IFitnessCoachClient client) => _client = client;

    [HttpPost("start-session")]
    public async Task<IActionResult> StartSession([FromBody] JsonElement? body, CancellationToken cancellationToken)
    {
        var b = body ?? default;
        var result = await _client.StartSessionAsync(b, cancellationToken);
        return Ok(result);
    }

    [HttpPost("analyze-frame")]
    public async Task<IActionResult> AnalyzeFrame([FromBody] JsonElement body, CancellationToken cancellationToken)
    {
        var result = await _client.AnalyzeFrameAsync(body, cancellationToken);
        return Ok(result);
    }

    [HttpPost("end-session")]
    public async Task<IActionResult> EndSession([FromBody] JsonElement body, CancellationToken cancellationToken)
    {
        var result = await _client.EndSessionAsync(body, cancellationToken);
        return Ok(result);
    }

    [HttpGet("session-summary/{sessionId}")]
    public async Task<IActionResult> GetSessionSummary([FromRoute] string sessionId, CancellationToken cancellationToken)
    {
        var result = await _client.GetSessionSummaryAsync(sessionId, cancellationToken);
        return Ok(result);
    }
}
