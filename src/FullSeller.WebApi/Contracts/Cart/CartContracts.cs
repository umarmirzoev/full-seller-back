namespace FullSeller.WebApi.Contracts.Cart;

public record CartItemDto(Guid Id, Guid ProductVariantId, Guid ProductId, string ProductName, int Quantity, decimal UnitPrice, decimal LineTotal);
public record CartDto(Guid Id, IReadOnlyList<CartItemDto> Items, int TotalWeightGrams, decimal ItemsTotal);

public record AddCartItemRequest(Guid ProductVariantId, int Quantity);
public record UpdateCartItemRequest(int Quantity);
public record BulkCartUploadRow(string Sku, int Quantity);
public record BulkCartUploadRequest(IReadOnlyList<BulkCartUploadRow> Rows);
public record BulkCartUploadResultRow(string Sku, int Quantity, bool Added, string? Error);
public record BulkCartUploadResponse(IReadOnlyList<BulkCartUploadResultRow> Results);
