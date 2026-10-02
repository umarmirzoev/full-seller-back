namespace FullSeller.Domain.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string entity, object key)
        : base($"{entity} с ключом '{key}' не найден(а).") { }
}
