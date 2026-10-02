namespace FullSeller.Domain.Interfaces.Services;

public interface IFileStorage
{
    /// <returns>Публичный URL загруженного файла.</returns>
    Task<string> UploadAsync(string fileName, Stream content, string contentType, CancellationToken ct = default);
    Task DeleteAsync(string fileUrl, CancellationToken ct = default);
}
