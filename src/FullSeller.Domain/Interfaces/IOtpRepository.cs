using FullSeller.Domain.Entities;

namespace FullSeller.Domain.Interfaces;

public interface IOtpRepository
{
    Task<Guid> CreateAsync(OtpCode otp, CancellationToken ct = default);
    Task<OtpCode?> GetActiveByPhoneAsync(string phone, CancellationToken ct = default);
    Task IncrementAttemptsAsync(Guid otpId, CancellationToken ct = default);
    Task MarkUsedAsync(Guid otpId, CancellationToken ct = default);
}
