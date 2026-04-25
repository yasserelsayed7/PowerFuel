using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using PowerFuel.Application.Media;

namespace PowerFuel.API.Services.Media;

public sealed class MediaUrlGenerator : IMediaUrlGenerator
{
    private readonly MediaSettings _settings;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public MediaUrlGenerator(
        IOptions<MediaSettings> settings,
        IHttpContextAccessor httpContextAccessor,
        IAssetsFileProvider assets)
    {
        _settings = settings.Value;
        _httpContextAccessor = httpContextAccessor;
    }

    public string? ProductImageUrl(string? imageFileName) => Build(imageFileName);
    public string? EquipmentImageUrl(string? imageFileName) => Build(imageFileName);
    public string? CoachProfileImageUrl(string? imageFileName) => Build(imageFileName);
    public string? TestimonialImageUrl(string? imageFileName) => Build(imageFileName);

    private string? Build(string? imageFileName)
    {
        if (string.IsNullOrWhiteSpace(imageFileName))
            return null;

        // إذا كان full URL قديم من localhost، استخرج اسم الملف بس
        if (imageFileName.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            imageFileName.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            var uri = new Uri(imageFileName);
            imageFileName = Path.GetFileName(uri.LocalPath);
        }

        var fileName = Path.GetFileName(imageFileName);

        if (string.IsNullOrWhiteSpace(fileName))
            return null;

        var encoded = Uri.EscapeDataString(fileName);

        // Primary: استخدم الـ PublicBaseUrl من الـ config
        var publicBase = _settings.PublicBaseUrl?.TrimEnd('/');
        if (!string.IsNullOrWhiteSpace(publicBase))
            return $"{publicBase}/assets/{encoded}";

        // Fallback: استخدم الـ request
        var request = _httpContextAccessor.HttpContext?.Request;
        if (request != null)
        {
            var scheme = request.Headers["X-Forwarded-Proto"].FirstOrDefault() ?? request.Scheme;
            var host = request.Headers["X-Forwarded-Host"].FirstOrDefault() ?? request.Host.Value;
            return $"{scheme}://{host}/assets/{encoded}";
        }

        return $"/assets/{encoded}";
    }
}