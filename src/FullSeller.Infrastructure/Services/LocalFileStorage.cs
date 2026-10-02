using FullSeller.Domain.Interfaces.Services;
using Microsoft.Extensions.Configuration;

namespace FullSeller.Infrastructure.Services;

/// <summary>Заготовка IFileStorage: сохраняет файлы на диск сервера (папка wwwroot/uploads).
/// В проде — заменить на реализацию под S3-совместимое хранилище, не меняя интерфейс.</summary>
public class LocalFileStorage : IFileStorage
{
    private readonly string _rootPath;
    private readonly string _publicBaseUrl;

    public LocalFileStorage(IConfiguration configuration)
    {
        _rootPath = configuration["FileStorage:LocalRootPath"] ?? "wwwroot/uploads";
        _publicBaseUrl = configuration["FileStorage:PublicBaseUrl"] ?? "/uploads";
        Directory.CreateDirectory(_rootPath);
    }

    public async Task<string> UploadAsync(string fileName, Stream content, string contentType, CancellationToken ct = default)
    {
        var safeName = $"{Guid.NewGuid():N}_{Path.GetFileName(fileName)}";
        var fullPath = Path.Combine(_rootPath, safeName);
        await using var fileStream = File.Create(fullPath);
        await content.CopyToAsync(fileStream, ct);
        return $"{_publicBaseUrl}/{safeName}";
    }

    public Task DeleteAsync(string fileUrl, CancellationToken ct = default)
    {
        var fileName = Path.GetFileName(fileUrl);
        var fullPath = Path.Combine(_rootPath, fileName);
        if (File.Exists(fullPath)) File.Delete(fullPath);
        return Task.CompletedTask;
    }
}
