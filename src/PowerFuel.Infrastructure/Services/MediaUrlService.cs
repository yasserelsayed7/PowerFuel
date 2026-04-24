using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using PowerFuel.Application.Common;

namespace PowerFuel.Infrastructure.Services;

public class MediaUrlService : IMediaUrlService
{
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public MediaUrlService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
    {
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
    }

    public string? ToAbsoluteUrl(string? maybeRelativeUrl)
    {
        if (string.IsNullOrWhiteSpace(maybeRelativeUrl))
            return null;

        if (Uri.TryCreate(maybeRelativeUrl, UriKind.Absolute, out var absolute))
            return absolute.ToString();

        var relative = maybeRelativeUrl.StartsWith("/") ? maybeRelativeUrl : "/" + maybeRelativeUrl;

        var configuredBase = _configuration["Media:PublicBaseUrl"];
        if (!string.IsNullOrWhiteSpace(configuredBase) &&
            Uri.TryCreate(configuredBase, UriKind.Absolute, out var baseUri))
        {
            return new Uri(baseUri, relative).ToString();
        }

        var http = _httpContextAccessor.HttpContext;
        if (http != null)
        {
            return $"{http.Request.Scheme}://{http.Request.Host}{relative}";
        }

        return relative;
    }
}

