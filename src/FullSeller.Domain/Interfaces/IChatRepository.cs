using FullSeller.Domain.Entities;

namespace FullSeller.Domain.Interfaces;

public interface IChatRepository
{
    Task<IReadOnlyList<ChatMessage>> GetHistoryAsync(Guid userId, CancellationToken ct = default);
    Task<Guid> AddMessageAsync(ChatMessage message, CancellationToken ct = default);
    Task MarkReadAsync(Guid userId, CancellationToken ct = default);
}
