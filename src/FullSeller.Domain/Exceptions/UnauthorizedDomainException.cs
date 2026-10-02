namespace FullSeller.Domain.Exceptions;

/// <summary>Доменное правило доступа нарушено (не путать с HTTP 401 — это про бизнес-правило,
/// например "нельзя оставить отзыв на не купленный товар").</summary>
public class UnauthorizedDomainException : Exception
{
    public UnauthorizedDomainException(string message) : base(message) { }
}
