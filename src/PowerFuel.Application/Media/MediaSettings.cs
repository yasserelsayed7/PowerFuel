namespace PowerFuel.Application.Media;

public sealed class MediaSettings
{
    public const string SectionName = "Media";

    /// <summary>Optional absolute path to the assets root. When empty, the API content-root "assets" folder is used.</summary>
    public string? AssetsPhysicalPath { get; set; }

    /// <summary>Optional public base URL (e.g. https://api.example.com). When empty, the current request is used.</summary>
    public string? PublicBaseUrl { get; set; }
}
