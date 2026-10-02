namespace FullSeller.Domain.Entities;

public class Brand
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? LogoUrl { get; set; }
}
