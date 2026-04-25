namespace PowerFuel.Application.Media;

/// <summary>Resolves whether a path relative to the assets root exists on disk.</summary>
public interface IAssetsFileProvider
{
    bool Exists(string relativePath);
}
