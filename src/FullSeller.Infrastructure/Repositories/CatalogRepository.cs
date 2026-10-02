using System.Text;
using System.Text.Json;
using FullSeller.Domain.Common;
using FullSeller.Domain.Entities;
using FullSeller.Domain.Interfaces;
using FullSeller.Infrastructure.Data;
using Npgsql;

namespace FullSeller.Infrastructure.Repositories;

public class CatalogRepository : SqlRepositoryBase, ICatalogRepository
{
    public CatalogRepository(ISqlConnectionFactory factory) : base(factory) { }
    public CatalogRepository(NpgsqlConnection connection, NpgsqlTransaction? transaction) : base(connection, transaction) { }

    private const string ProductColumns = """
        Id, Name, CategoryId, BrandId, Description, Composition,
        BaseSku, WeightGrams, ImageUrlsJson, Rating, ReviewsCount, IsActive,
        MinPrice, OldPrice, IsNew, IsHit, CreatedAt
        """;

    public Task<IReadOnlyList<Category>> GetCategoriesAsync(CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        using var cmd = CreateCommand(conn, tx, "SELECT Id, Name, Slug, ParentCategoryId FROM Categories ORDER BY Name");
        using var reader = await cmd.ExecuteReaderAsync(ct);
        var list = new List<Category>();
        while (await reader.ReadAsync(ct))
        {
            list.Add(new Category
            {
                Id = reader.GetGuid(0),
                Name = reader.GetString(1),
                Slug = reader.GetString(2),
                ParentCategoryId = reader.IsDBNull(3) ? null : reader.GetGuid(3),
            });
        }
        return (IReadOnlyList<Category>)list;
    }, ct);

    public Task<IReadOnlyList<Brand>> GetBrandsAsync(CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        using var cmd = CreateCommand(conn, tx, "SELECT Id, Name, LogoUrl FROM Brands ORDER BY Name");
        using var reader = await cmd.ExecuteReaderAsync(ct);
        var list = new List<Brand>();
        while (await reader.ReadAsync(ct))
        {
            list.Add(new Brand
            {
                Id = reader.GetGuid(0),
                Name = reader.GetString(1),
                LogoUrl = reader.IsDBNull(2) ? null : reader.GetString(2),
            });
        }
        return (IReadOnlyList<Brand>)list;
    }, ct);

    public Task<PagedResult<Product>> SearchProductsAsync(CatalogSearchFilter filter, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        var where = new StringBuilder("WHERE p.IsActive = TRUE");
        if (filter.CategoryId is not null) where.Append(" AND p.CategoryId = @CategoryId");
        if (filter.BrandId is not null) where.Append(" AND p.BrandId = @BrandId");
        if (!string.IsNullOrWhiteSpace(filter.SearchText)) where.Append(" AND p.Name ILIKE @SearchText");
        if (filter.MinPrice is not null) where.Append(" AND p.MinPrice >= @MinPrice");
        if (filter.MaxPrice is not null) where.Append(" AND p.MinPrice <= @MaxPrice");

        var orderBy = filter.SortBy switch
        {
            "price_asc" => "p.MinPrice ASC",
            "price_desc" => "p.MinPrice DESC",
            "newest" => "p.CreatedAt DESC",
            _ => "p.Rating DESC, p.ReviewsCount DESC",
        };

        var countSql = $"SELECT COUNT(*) FROM Products p {where}";
        using (var countCmd = CreateCommand(conn, tx, countSql))
        {
            AddFilterParams(countCmd, filter);
            var total = (int)(long)(await countCmd.ExecuteScalarAsync(ct) ?? 0L);

            var pageSql = $"""
                SELECT {ProductColumns}
                FROM Products p
                {where}
                ORDER BY {orderBy}
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
                """;
            using var pageCmd = CreateCommand(conn, tx, pageSql);
            AddFilterParams(pageCmd, filter);
            pageCmd.Parameters.AddWithValue("@Offset", (filter.Page - 1) * filter.PageSize);
            pageCmd.Parameters.AddWithValue("@PageSize", filter.PageSize);

            using var reader = await pageCmd.ExecuteReaderAsync(ct);
            var items = new List<Product>();
            while (await reader.ReadAsync(ct)) items.Add(MapProduct(reader));

            return new PagedResult<Product> { Items = items, TotalCount = total, Page = filter.Page, PageSize = filter.PageSize };
        }
    }, ct);

    private static void AddFilterParams(NpgsqlCommand cmd, CatalogSearchFilter filter)
    {
        if (filter.CategoryId is not null) cmd.Parameters.AddWithValue("@CategoryId", filter.CategoryId.Value);
        if (filter.BrandId is not null) cmd.Parameters.AddWithValue("@BrandId", filter.BrandId.Value);
        if (!string.IsNullOrWhiteSpace(filter.SearchText)) cmd.Parameters.AddWithValue("@SearchText", $"%{filter.SearchText}%");
        if (filter.MinPrice is not null) cmd.Parameters.AddWithValue("@MinPrice", filter.MinPrice.Value);
        if (filter.MaxPrice is not null) cmd.Parameters.AddWithValue("@MaxPrice", filter.MaxPrice.Value);
    }

    public Task<Product?> GetProductByIdAsync(Guid id, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        using var cmd = CreateCommand(conn, tx, $"SELECT {ProductColumns} FROM Products WHERE Id = @Id");
        cmd.Parameters.AddWithValue("@Id", id);
        using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? MapProduct(reader) : null;
    }, ct);

    public Task<IReadOnlyList<ProductVariant>> GetVariantsByProductIdAsync(Guid productId, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        using var cmd = CreateCommand(conn, tx,
            "SELECT Id, ProductId, Size, Color, Sku, StockQuantity FROM ProductVariants WHERE ProductId = @ProductId");
        cmd.Parameters.AddWithValue("@ProductId", productId);
        using var reader = await cmd.ExecuteReaderAsync(ct);
        var list = new List<ProductVariant>();
        while (await reader.ReadAsync(ct)) list.Add(MapVariant(reader));
        return (IReadOnlyList<ProductVariant>)list;
    }, ct);

    public Task<ProductVariant?> GetVariantByIdAsync(Guid variantId, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        using var cmd = CreateCommand(conn, tx,
            "SELECT Id, ProductId, Size, Color, Sku, StockQuantity FROM ProductVariants WHERE Id = @Id");
        cmd.Parameters.AddWithValue("@Id", variantId);
        using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? MapVariant(reader) : null;
    }, ct);

    public Task<IReadOnlyList<PriceTier>> GetPriceTiersAsync(Guid productId, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        using var cmd = CreateCommand(conn, tx,
            "SELECT Id, ProductId, MinQuantity, PricePerUnit FROM PriceTiers WHERE ProductId = @ProductId ORDER BY MinQuantity");
        cmd.Parameters.AddWithValue("@ProductId", productId);
        using var reader = await cmd.ExecuteReaderAsync(ct);
        var list = new List<PriceTier>();
        while (await reader.ReadAsync(ct))
        {
            list.Add(new PriceTier
            {
                Id = reader.GetGuid(0),
                ProductId = reader.GetGuid(1),
                MinQuantity = reader.GetInt32(2),
                PricePerUnit = reader.GetDecimal(3),
            });
        }
        return (IReadOnlyList<PriceTier>)list;
    }, ct);

    public Task DecrementStockAsync(Guid variantId, int quantity, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        using var cmd = CreateCommand(conn, tx, """
            UPDATE ProductVariants SET StockQuantity = StockQuantity - @Quantity
            WHERE Id = @Id AND StockQuantity >= @Quantity
            """);
        cmd.Parameters.AddWithValue("@Id", variantId);
        cmd.Parameters.AddWithValue("@Quantity", quantity);
        var affected = await cmd.ExecuteNonQueryAsync(ct);
        if (affected == 0)
            throw new InvalidOperationException($"Недостаточно остатка для варианта {variantId}.");
    }, ct);

    // =====================================================================================
    // Админ-панель: категории, бренды, товары, варианты, price tiers (ТЗ п.10 «Товары»).
    // =====================================================================================

    public Task<Guid> CreateCategoryAsync(Category category, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        category.Id = category.Id == Guid.Empty ? Guid.NewGuid() : category.Id;
        using var cmd = CreateCommand(conn, tx,
            "INSERT INTO Categories (Id, Name, Slug, ParentCategoryId) VALUES (@Id, @Name, @Slug, @ParentCategoryId)");
        cmd.Parameters.AddWithValue("@Id", category.Id);
        cmd.Parameters.AddWithValue("@Name", category.Name);
        cmd.Parameters.AddWithValue("@Slug", category.Slug);
        cmd.Parameters.AddWithValue("@ParentCategoryId", (object?)category.ParentCategoryId ?? DBNull.Value);
        await cmd.ExecuteNonQueryAsync(ct);
        return category.Id;
    }, ct);

    public Task UpdateCategoryAsync(Category category, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        using var cmd = CreateCommand(conn, tx,
            "UPDATE Categories SET Name = @Name, Slug = @Slug, ParentCategoryId = @ParentCategoryId WHERE Id = @Id");
        cmd.Parameters.AddWithValue("@Id", category.Id);
        cmd.Parameters.AddWithValue("@Name", category.Name);
        cmd.Parameters.AddWithValue("@Slug", category.Slug);
        cmd.Parameters.AddWithValue("@ParentCategoryId", (object?)category.ParentCategoryId ?? DBNull.Value);
        await cmd.ExecuteNonQueryAsync(ct);
    }, ct);

    public Task DeleteCategoryAsync(Guid id, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        using var cmd = CreateCommand(conn, tx, "DELETE FROM Categories WHERE Id = @Id");
        cmd.Parameters.AddWithValue("@Id", id);
        await cmd.ExecuteNonQueryAsync(ct);
    }, ct);

    public Task<Guid> CreateBrandAsync(Brand brand, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        brand.Id = brand.Id == Guid.Empty ? Guid.NewGuid() : brand.Id;
        using var cmd = CreateCommand(conn, tx, "INSERT INTO Brands (Id, Name, LogoUrl) VALUES (@Id, @Name, @LogoUrl)");
        cmd.Parameters.AddWithValue("@Id", brand.Id);
        cmd.Parameters.AddWithValue("@Name", brand.Name);
        cmd.Parameters.AddWithValue("@LogoUrl", (object?)brand.LogoUrl ?? DBNull.Value);
        await cmd.ExecuteNonQueryAsync(ct);
        return brand.Id;
    }, ct);

    public Task UpdateBrandAsync(Brand brand, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        using var cmd = CreateCommand(conn, tx, "UPDATE Brands SET Name = @Name, LogoUrl = @LogoUrl WHERE Id = @Id");
        cmd.Parameters.AddWithValue("@Id", brand.Id);
        cmd.Parameters.AddWithValue("@Name", brand.Name);
        cmd.Parameters.AddWithValue("@LogoUrl", (object?)brand.LogoUrl ?? DBNull.Value);
        await cmd.ExecuteNonQueryAsync(ct);
    }, ct);

    public Task DeleteBrandAsync(Guid id, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        using var cmd = CreateCommand(conn, tx, "DELETE FROM Brands WHERE Id = @Id");
        cmd.Parameters.AddWithValue("@Id", id);
        await cmd.ExecuteNonQueryAsync(ct);
    }, ct);

    public Task<PagedResult<Product>> GetAllProductsForAdminAsync(int page, int pageSize, string? search, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        var where = string.IsNullOrWhiteSpace(search) ? "" : "WHERE Name ILIKE @Search";

        using (var countCmd = CreateCommand(conn, tx, $"SELECT COUNT(*) FROM Products {where}"))
        {
            if (!string.IsNullOrWhiteSpace(search)) countCmd.Parameters.AddWithValue("@Search", $"%{search}%");
            var total = (int)(long)(await countCmd.ExecuteScalarAsync(ct) ?? 0L);

            using var pageCmd = CreateCommand(conn, tx, $"""
                SELECT {ProductColumns} FROM Products {where}
                ORDER BY CreatedAt DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
                """);
            if (!string.IsNullOrWhiteSpace(search)) pageCmd.Parameters.AddWithValue("@Search", $"%{search}%");
            pageCmd.Parameters.AddWithValue("@Offset", (page - 1) * pageSize);
            pageCmd.Parameters.AddWithValue("@PageSize", pageSize);

            using var reader = await pageCmd.ExecuteReaderAsync(ct);
            var items = new List<Product>();
            while (await reader.ReadAsync(ct)) items.Add(MapProduct(reader));
            return new PagedResult<Product> { Items = items, TotalCount = total, Page = page, PageSize = pageSize };
        }
    }, ct);

    public Task<Guid> CreateProductAsync(Product product, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        product.Id = product.Id == Guid.Empty ? Guid.NewGuid() : product.Id;
        if (product.CreatedAt == default) product.CreatedAt = DateTime.UtcNow;
        using var cmd = CreateCommand(conn, tx, """
            INSERT INTO Products (Id, Name, CategoryId, BrandId, Description, Composition, BaseSku, WeightGrams,
                                   ImageUrlsJson, Rating, ReviewsCount, IsActive, MinPrice, OldPrice, IsNew, IsHit, CreatedAt)
            VALUES (@Id, @Name, @CategoryId, @BrandId, @Description, @Composition, @BaseSku, @WeightGrams,
                    @ImageUrlsJson, 0, 0, @IsActive, @MinPrice, @OldPrice, @IsNew, @IsHit, @CreatedAt)
            """);
        BindProductParams(cmd, product);
        cmd.Parameters.AddWithValue("@CreatedAt", product.CreatedAt);
        await cmd.ExecuteNonQueryAsync(ct);
        return product.Id;
    }, ct);

    public Task UpdateProductAsync(Product product, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        using var cmd = CreateCommand(conn, tx, """
            UPDATE Products SET
                Name = @Name, CategoryId = @CategoryId, BrandId = @BrandId, Description = @Description,
                Composition = @Composition, BaseSku = @BaseSku, WeightGrams = @WeightGrams, ImageUrlsJson = @ImageUrlsJson,
                IsActive = @IsActive, MinPrice = @MinPrice, OldPrice = @OldPrice, IsNew = @IsNew, IsHit = @IsHit
            WHERE Id = @Id
            """);
        BindProductParams(cmd, product);
        await cmd.ExecuteNonQueryAsync(ct);
    }, ct);

    private static void BindProductParams(NpgsqlCommand cmd, Product product)
    {
        cmd.Parameters.AddWithValue("@Id", product.Id);
        cmd.Parameters.AddWithValue("@Name", product.Name);
        cmd.Parameters.AddWithValue("@CategoryId", product.CategoryId);
        cmd.Parameters.AddWithValue("@BrandId", (object?)product.BrandId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Description", (object?)product.Description ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Composition", (object?)product.Composition ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@BaseSku", product.BaseSku);
        cmd.Parameters.AddWithValue("@WeightGrams", product.WeightGrams);
        cmd.Parameters.AddWithValue("@ImageUrlsJson", JsonSerializer.Serialize(product.ImageUrls));
        cmd.Parameters.AddWithValue("@IsActive", product.IsActive);
        cmd.Parameters.AddWithValue("@MinPrice", product.MinPrice);
        cmd.Parameters.AddWithValue("@OldPrice", (object?)product.OldPrice ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@IsNew", product.IsNew);
        cmd.Parameters.AddWithValue("@IsHit", product.IsHit);
    }

    public Task SetProductActiveAsync(Guid id, bool isActive, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        using var cmd = CreateCommand(conn, tx, "UPDATE Products SET IsActive = @IsActive WHERE Id = @Id");
        cmd.Parameters.AddWithValue("@Id", id);
        cmd.Parameters.AddWithValue("@IsActive", isActive);
        await cmd.ExecuteNonQueryAsync(ct);
    }, ct);

    public Task DeleteProductAsync(Guid id, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        // Порядок важен из-за внешних ключей: сначала дочерние строки (варианты, price tiers), затем товар.
        // Если на товар уже есть заказы/отзывы (FK без ON DELETE CASCADE), Postgres сам откажет — тогда
        // с точки зрения бизнес-логики правильнее скрыть товар (SetProductActiveAsync), а не удалять.
        using (var variants = CreateCommand(conn, tx, "DELETE FROM ProductVariants WHERE ProductId = @Id"))
        {
            variants.Parameters.AddWithValue("@Id", id);
            await variants.ExecuteNonQueryAsync(ct);
        }
        using (var tiers = CreateCommand(conn, tx, "DELETE FROM PriceTiers WHERE ProductId = @Id"))
        {
            tiers.Parameters.AddWithValue("@Id", id);
            await tiers.ExecuteNonQueryAsync(ct);
        }
        using var product = CreateCommand(conn, tx, "DELETE FROM Products WHERE Id = @Id");
        product.Parameters.AddWithValue("@Id", id);
        await product.ExecuteNonQueryAsync(ct);
    }, ct);

    public Task<Guid> CreateVariantAsync(ProductVariant variant, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        variant.Id = variant.Id == Guid.Empty ? Guid.NewGuid() : variant.Id;
        using var cmd = CreateCommand(conn, tx, """
            INSERT INTO ProductVariants (Id, ProductId, Size, Color, Sku, StockQuantity)
            VALUES (@Id, @ProductId, @Size, @Color, @Sku, @StockQuantity)
            """);
        BindVariantParams(cmd, variant);
        await cmd.ExecuteNonQueryAsync(ct);
        return variant.Id;
    }, ct);

    public Task UpdateVariantAsync(ProductVariant variant, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        using var cmd = CreateCommand(conn, tx, """
            UPDATE ProductVariants SET Size = @Size, Color = @Color, Sku = @Sku, StockQuantity = @StockQuantity
            WHERE Id = @Id
            """);
        BindVariantParams(cmd, variant);
        await cmd.ExecuteNonQueryAsync(ct);
    }, ct);

    private static void BindVariantParams(NpgsqlCommand cmd, ProductVariant variant)
    {
        cmd.Parameters.AddWithValue("@Id", variant.Id);
        cmd.Parameters.AddWithValue("@ProductId", variant.ProductId);
        cmd.Parameters.AddWithValue("@Size", (object?)variant.Size ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Color", (object?)variant.Color ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Sku", variant.Sku);
        cmd.Parameters.AddWithValue("@StockQuantity", variant.StockQuantity);
    }

    public Task DeleteVariantAsync(Guid id, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        using var cmd = CreateCommand(conn, tx, "DELETE FROM ProductVariants WHERE Id = @Id");
        cmd.Parameters.AddWithValue("@Id", id);
        await cmd.ExecuteNonQueryAsync(ct);
    }, ct);

    public Task SetStockAsync(Guid variantId, int stockQuantity, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        using var cmd = CreateCommand(conn, tx, "UPDATE ProductVariants SET StockQuantity = @StockQuantity WHERE Id = @Id");
        cmd.Parameters.AddWithValue("@Id", variantId);
        cmd.Parameters.AddWithValue("@StockQuantity", stockQuantity);
        await cmd.ExecuteNonQueryAsync(ct);
    }, ct);

    public Task ReplacePriceTiersAsync(Guid productId, IReadOnlyList<PriceTier> tiers, CancellationToken ct = default) => RunAsync(async (conn, tx) =>
    {
        using (var del = CreateCommand(conn, tx, "DELETE FROM PriceTiers WHERE ProductId = @ProductId"))
        {
            del.Parameters.AddWithValue("@ProductId", productId);
            await del.ExecuteNonQueryAsync(ct);
        }

        foreach (var tier in tiers.OrderBy(t => t.MinQuantity))
        {
            using var insert = CreateCommand(conn, tx, """
                INSERT INTO PriceTiers (Id, ProductId, MinQuantity, PricePerUnit)
                VALUES (@Id, @ProductId, @MinQuantity, @PricePerUnit)
                """);
            insert.Parameters.AddWithValue("@Id", tier.Id == Guid.Empty ? Guid.NewGuid() : tier.Id);
            insert.Parameters.AddWithValue("@ProductId", productId);
            insert.Parameters.AddWithValue("@MinQuantity", tier.MinQuantity);
            insert.Parameters.AddWithValue("@PricePerUnit", tier.PricePerUnit);
            await insert.ExecuteNonQueryAsync(ct);
        }

        // Products.MinPrice денормализован от самой низкой оптовой цены — держим его в синхроне при каждом изменении сетки.
        var minPrice = tiers.Count > 0 ? tiers.Min(t => t.PricePerUnit) : 0m;
        using var updateProduct = CreateCommand(conn, tx, "UPDATE Products SET MinPrice = @MinPrice WHERE Id = @ProductId");
        updateProduct.Parameters.AddWithValue("@ProductId", productId);
        updateProduct.Parameters.AddWithValue("@MinPrice", minPrice);
        await updateProduct.ExecuteNonQueryAsync(ct);
    }, ct);

    private static Product MapProduct(NpgsqlDataReader r) => new()
    {
        Id = r.GetGuid(0),
        Name = r.GetString(1),
        CategoryId = r.GetGuid(2),
        BrandId = r.IsDBNull(3) ? null : r.GetGuid(3),
        Description = r.IsDBNull(4) ? null : r.GetString(4),
        Composition = r.IsDBNull(5) ? null : r.GetString(5),
        BaseSku = r.GetString(6),
        WeightGrams = r.GetInt32(7),
        ImageUrls = r.IsDBNull(8) ? new List<string>() : JsonSerializer.Deserialize<List<string>>(r.GetString(8)) ?? new(),
        Rating = r.GetDecimal(9),
        ReviewsCount = r.GetInt32(10),
        IsActive = r.GetBoolean(11),
        MinPrice = r.GetDecimal(12),
        OldPrice = r.IsDBNull(13) ? null : r.GetDecimal(13),
        IsNew = r.GetBoolean(14),
        IsHit = r.GetBoolean(15),
        CreatedAt = r.GetDateTime(16),
    };

    private static ProductVariant MapVariant(NpgsqlDataReader r) => new()
    {
        Id = r.GetGuid(0),
        ProductId = r.GetGuid(1),
        Size = r.IsDBNull(2) ? null : r.GetString(2),
        Color = r.IsDBNull(3) ? null : r.GetString(3),
        Sku = r.GetString(4),
        StockQuantity = r.GetInt32(5),
    };
}
