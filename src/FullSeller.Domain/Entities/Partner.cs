namespace FullSeller.Domain.Entities;

/// <summary>
/// Партнёр (производитель/оптовый поставщик), у которого есть собственный кабинет для загрузки
/// товаров в каталог. Вход отдельный — логин+пароль, выдаётся вручную администратором
/// (не через клиентский OTP-флоу, см. <see cref="User"/>).
/// </summary>
public class Partner
{
    public Guid Id { get; set; }
    public string CompanyName { get; set; } = default!;
    public string ContactName { get; set; } = default!;
    public string Phone { get; set; } = default!;
    public string Login { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
