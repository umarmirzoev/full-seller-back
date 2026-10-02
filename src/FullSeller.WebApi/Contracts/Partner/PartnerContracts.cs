namespace FullSeller.WebApi.Contracts.Partner;

public record PartnerLoginRequest(string Login, string Password);

public record PartnerProfileDto(Guid Id, string CompanyName, string ContactName, string Phone, string Login, bool IsActive, DateTime CreatedAt);

/// <summary>Создание учётной записи партнёра администратором (POST api/admin/partners) — логин и пароль
/// выдаются вручную, самостоятельной регистрации у партнёров нет.</summary>
public record CreatePartnerRequest(string CompanyName, string ContactName, string Phone, string Login, string Password);
