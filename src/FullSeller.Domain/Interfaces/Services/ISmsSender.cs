namespace FullSeller.Domain.Interfaces.Services;

public interface ISmsSender
{
    Task SendOtpAsync(string phone, string code, CancellationToken ct = default);
}
