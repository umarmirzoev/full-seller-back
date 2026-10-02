using FullSeller.Domain.Entities;

namespace FullSeller.Domain.Interfaces.Services;

public record TokenPair(string AccessToken, string RefreshToken, DateTime AccessTokenExpiresAt);

public interface ITokenService
{
    TokenPair IssueTokens(User user);

    /// <summary>Отдельный набор токенов для партнёрского кабинета (роль "Partner" в claim "role") —
    /// партнёры не являются записями в Users, поэтому IssueTokens(User) им не подходит.</summary>
    TokenPair IssuePartnerTokens(Guid partnerId, string login);
    string HashToken(string rawToken);
}
