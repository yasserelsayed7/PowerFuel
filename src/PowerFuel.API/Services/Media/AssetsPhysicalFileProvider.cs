using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using PowerFuel.Application.Media;

namespace PowerFuel.API.Services.Media;

public sealed class AssetsPhysicalFileProvider : IAssetsFileProvider, IDisposable
{
    private readonly PhysicalFileProvider _provider;

    public AssetsPhysicalFileProvider(IOptions<MediaSettings> options, IWebHostEnvironment env)
    {
        var path = options.Value.AssetsPhysicalPath;
        if (string.IsNullOrWhiteSpace(path))
            path = Path.Combine(env.ContentRootPath, "assets");
        Directory.CreateDirectory(path);
        _provider = new PhysicalFileProvider(path);
    }

    public bool Exists(string relativePath)
    {
        try
        {
            return _provider.GetFileInfo(relativePath).Exists;
        }
        catch
        {
            return false;
        }
    }

    public void Dispose() => _provider.Dispose();
}
