namespace FullSeller.Domain.Interfaces.Services;

public interface IPushNotifier
{
    Task SendAsync(Guid userId, string title, string body, CancellationToken ct = default);
}
