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
    public Task<IActionResult> StartSession([FromBody] StartSessionRequestDto? body, CancellationToken cancellationToken) =>
        InvokeAsync(() => _client.StartSessionAsync(body ?? new StartSessionRequestDto(), cancellationToken));

    [HttpPost("analyze-frame")]
    public Task<IActionResult> AnalyzeFrame([FromBody] AnalyzeFrameRequestDto body, CancellationToken cancellationToken) =>
        InvokeAsync(() => _client.AnalyzeFrameAsync(body, cancellationToken));

    [HttpPost("end-session")]
    public Task<IActionResult> EndSession([FromBody] EndSessionRequestDto body, CancellationToken cancellationToken) =>
        InvokeAsync(() => _client.EndSessionAsync(body, cancellationToken));

    [HttpGet("session-summary/{sessionId}")]
    public Task<IActionResult> GetSessionSummary([FromRoute] string sessionId, CancellationToken cancellationToken) =>
        InvokeAsync(() => _client.GetSessionSummaryAsync(sessionId, cancellationToken));

    private async Task<IActionResult> InvokeAsync<T>(Func<Task<T>> operation)
    {
        try
        {
            return Ok(await operation().ConfigureAwait(false));
        }
        catch (FitnessCoachApiException ex)
        {
            return Problem(
                title: "Fitness coach service error",
                detail: ex.ResponseBody,
                statusCode: (int)ex.StatusCode);
        }
    }
}
