namespace PowerFuel.Application.Common;

public interface IMediaUrlService
{
    /// <summary>
    /// Converts a stored relative path (e.g. "/assets/x.png") into an absolute URL.
    /// If the input is already absolute, it is returned as-is.
    /// </summary>
    string? ToAbsoluteUrl(string? maybeRelativeUrl);
}

