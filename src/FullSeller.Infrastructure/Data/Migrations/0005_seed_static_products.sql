-- 0005_seed_static_products.sql
-- Сид каталога сайта (бывшие STATIC_SOCKS / STATIC_REGULAR_SOCKS / STATIC_TSHIRTS / STATIC_UNDERWEAR
-- из assets/js/app.js) в реальные таблицы Products/Brands/ProductVariants/PriceTiers,
-- чтобы эти товары появились в публичном Catalog API (и соответственно во Flutter-приложении).
-- Применяется автоматически MigrationRunner'ом при старте бэкенда (один раз, отслеживается в __SchemaMigrations).

-- 1) Бренды (создаём только если ещё не существуют с таким именем)
INSERT INTO Brands (Id, Name, LogoUrl)
SELECT 'b16a0227-7999-5092-b4b3-569ac8a1320b'::uuid, 'Adidas', NULL
WHERE NOT EXISTS (SELECT 1 FROM Brands WHERE Name = 'Adidas');
INSERT INTO Brands (Id, Name, LogoUrl)
SELECT 'c8cf75eb-12d0-58be-9220-03e4613110bf'::uuid, 'Fila', NULL
WHERE NOT EXISTS (SELECT 1 FROM Brands WHERE Name = 'Fila');
INSERT INTO Brands (Id, Name, LogoUrl)
SELECT 'ec2615c8-3fc0-5b89-9992-0c09878abfc6'::uuid, 'Nike', NULL
WHERE NOT EXISTS (SELECT 1 FROM Brands WHERE Name = 'Nike');
INSERT INTO Brands (Id, Name, LogoUrl)
SELECT 'fa6c347d-9db7-5f6e-bece-54c7afaffc8e'::uuid, 'Puma', NULL
WHERE NOT EXISTS (SELECT 1 FROM Brands WHERE Name = 'Puma');
INSERT INTO Brands (Id, Name, LogoUrl)
SELECT '36bc313e-13c3-5412-9330-722533ef58b7'::uuid, 'Обычные', NULL
WHERE NOT EXISTS (SELECT 1 FROM Brands WHERE Name = 'Обычные');

-- 2) Товары + варианты + ценовые уровни (розница, 1 тир = обычная цена сайта)
-- static1: Nike Everyday (Белые)
INSERT INTO Products (Id, Name, CategoryId, BrandId, Description, Composition, BaseSku, WeightGrams, ImageUrlsJson, Rating, ReviewsCount, IsActive, MinPrice, AudienceTag, CreatedAt)
SELECT '2ef2701c-2bd7-5c00-9145-2b7de1644a8b'::uuid, 'Nike Everyday (Белые)', 'ef45badd-483e-40a1-8040-00cde2f92550'::uuid, 'ec2615c8-3fc0-5b89-9992-0c09878abfc6'::uuid, 'Классические белые носки Nike, технология Dri-FIT.', 'Хлопок 75%, Полиэстер 22%', 'WEB-STATIC1', 0, '["https://full-seller.com/assets/images/0%20%285%29.jpg"]'::text, 4.5, 0, TRUE, 13, 'male', now()
WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Id = '2ef2701c-2bd7-5c00-9145-2b7de1644a8b'::uuid);
INSERT INTO PriceTiers (Id, ProductId, MinQuantity, PricePerUnit)
SELECT '195aba01-4d13-5bd9-847e-1b19ed728d23'::uuid, '2ef2701c-2bd7-5c00-9145-2b7de1644a8b'::uuid, 1, 13
WHERE NOT EXISTS (SELECT 1 FROM PriceTiers WHERE ProductId = '2ef2701c-2bd7-5c00-9145-2b7de1644a8b'::uuid);
INSERT INTO ProductVariants (Id, ProductId, Size, Color, Sku, StockQuantity)
SELECT '40122a57-a673-5661-a19d-35b497c8107d'::uuid, '2ef2701c-2bd7-5c00-9145-2b7de1644a8b'::uuid, '41-43', NULL, 'WEB-STATIC1-V1', 500
WHERE NOT EXISTS (SELECT 1 FROM ProductVariants WHERE Sku = 'WEB-STATIC1-V1');

-- static2: Nike Everyday (Черные)
INSERT INTO Products (Id, Name, CategoryId, BrandId, Description, Composition, BaseSku, WeightGrams, ImageUrlsJson, Rating, ReviewsCount, IsActive, MinPrice, AudienceTag, CreatedAt)
SELECT '9c3978d9-e3a5-5103-8842-e8d5525fb5cf'::uuid, 'Nike Everyday (Черные)', 'ef45badd-483e-40a1-8040-00cde2f92550'::uuid, 'ec2615c8-3fc0-5b89-9992-0c09878abfc6'::uuid, 'Спортивные черные носки Nike, отличная вентиляция.', 'Хлопок 75%, Полиэстер 22%', 'WEB-STATIC2', 0, '["https://full-seller.com/assets/images/0%20%286%29.jpg"]'::text, 4.6, 0, TRUE, 13, 'male', now()
WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Id = '9c3978d9-e3a5-5103-8842-e8d5525fb5cf'::uuid);
INSERT INTO PriceTiers (Id, ProductId, MinQuantity, PricePerUnit)
SELECT '1063f4d0-392f-5297-b7fa-2af09c6c9396'::uuid, '9c3978d9-e3a5-5103-8842-e8d5525fb5cf'::uuid, 1, 13
WHERE NOT EXISTS (SELECT 1 FROM PriceTiers WHERE ProductId = '9c3978d9-e3a5-5103-8842-e8d5525fb5cf'::uuid);
INSERT INTO ProductVariants (Id, ProductId, Size, Color, Sku, StockQuantity)
SELECT 'ee3a59ae-474e-5a52-99a0-686cb3740802'::uuid, '9c3978d9-e3a5-5103-8842-e8d5525fb5cf'::uuid, '41-43', NULL, 'WEB-STATIC2-V1', 500
WHERE NOT EXISTS (SELECT 1 FROM ProductVariants WHERE Sku = 'WEB-STATIC2-V1');

-- static3: Nike Everyday (Серые)
INSERT INTO Products (Id, Name, CategoryId, BrandId, Description, Composition, BaseSku, WeightGrams, ImageUrlsJson, Rating, ReviewsCount, IsActive, MinPrice, AudienceTag, CreatedAt)
SELECT '1d236414-5579-5223-bff9-c7a2d46c6827'::uuid, 'Nike Everyday (Серые)', 'ef45badd-483e-40a1-8040-00cde2f92550'::uuid, 'ec2615c8-3fc0-5b89-9992-0c09878abfc6'::uuid, 'Универсальные серые носки Nike, Dri-FIT и мягкий пояс.', 'Хлопок 75%, Полиэстер 22%', 'WEB-STATIC3', 0, '["https://full-seller.com/assets/images/0%20%2819%29.jpg"]'::text, 4.4, 0, TRUE, 13, 'male', now()
WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Id = '1d236414-5579-5223-bff9-c7a2d46c6827'::uuid);
INSERT INTO PriceTiers (Id, ProductId, MinQuantity, PricePerUnit)
SELECT '0cb99513-64f0-5f26-8767-d236613cd312'::uuid, '1d236414-5579-5223-bff9-c7a2d46c6827'::uuid, 1, 13
WHERE NOT EXISTS (SELECT 1 FROM PriceTiers WHERE ProductId = '1d236414-5579-5223-bff9-c7a2d46c6827'::uuid);
INSERT INTO ProductVariants (Id, ProductId, Size, Color, Sku, StockQuantity)
SELECT 'adf3cb72-d014-58bd-ae24-e688110617ae'::uuid, '1d236414-5579-5223-bff9-c7a2d46c6827'::uuid, '41-43', NULL, 'WEB-STATIC3-V1', 500
WHERE NOT EXISTS (SELECT 1 FROM ProductVariants WHERE Sku = 'WEB-STATIC3-V1');

-- static4: Nike Everyday (Синие)
INSERT INTO Products (Id, Name, CategoryId, BrandId, Description, Composition, BaseSku, WeightGrams, ImageUrlsJson, Rating, ReviewsCount, IsActive, MinPrice, AudienceTag, CreatedAt)
SELECT 'bf070300-ac9a-584f-880e-4c8cafde7550'::uuid, 'Nike Everyday (Синие)', 'ef45badd-483e-40a1-8040-00cde2f92550'::uuid, 'ec2615c8-3fc0-5b89-9992-0c09878abfc6'::uuid, 'Тёмно-синие носки Nike, повседневный комфорт.', 'Хлопок 75%, Полиэстер 22%', 'WEB-STATIC4', 0, '["https://full-seller.com/assets/images/0%20%2812%29.jpg"]'::text, 4.3, 0, TRUE, 13, 'male', now()
WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Id = 'bf070300-ac9a-584f-880e-4c8cafde7550'::uuid);
INSERT INTO PriceTiers (Id, ProductId, MinQuantity, PricePerUnit)
SELECT 'a9df464d-b079-5362-9244-23bac1a1e5fe'::uuid, 'bf070300-ac9a-584f-880e-4c8cafde7550'::uuid, 1, 13
WHERE NOT EXISTS (SELECT 1 FROM PriceTiers WHERE ProductId = 'bf070300-ac9a-584f-880e-4c8cafde7550'::uuid);
INSERT INTO ProductVariants (Id, ProductId, Size, Color, Sku, StockQuantity)
SELECT '903bb824-6768-5d2a-abb2-0b4fc97ceddb'::uuid, 'bf070300-ac9a-584f-880e-4c8cafde7550'::uuid, '41-43', NULL, 'WEB-STATIC4-V1', 500
WHERE NOT EXISTS (SELECT 1 FROM ProductVariants WHERE Sku = 'WEB-STATIC4-V1');

-- static5: Adidas Cushioned (Белые)
INSERT INTO Products (Id, Name, CategoryId, BrandId, Description, Composition, BaseSku, WeightGrams, ImageUrlsJson, Rating, ReviewsCount, IsActive, MinPrice, AudienceTag, CreatedAt)
SELECT 'de4ff6f0-ab03-5dce-a88e-d529310d0ae1'::uuid, 'Adidas Cushioned (Белые)', 'ef45badd-483e-40a1-8040-00cde2f92550'::uuid, 'b16a0227-7999-5092-b4b3-569ac8a1320b'::uuid, 'Белые носки Adidas с мягкой подошвой.', 'Хлопок 60%, Полиэстер 36%', 'WEB-STATIC5', 0, '["https://full-seller.com/assets/images/0%20%284%29.jpg"]'::text, 4.5, 0, TRUE, 13, 'male', now()
WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Id = 'de4ff6f0-ab03-5dce-a88e-d529310d0ae1'::uuid);
INSERT INTO PriceTiers (Id, ProductId, MinQuantity, PricePerUnit)
SELECT '5897d70c-e5ec-5f37-ac30-f2fc832c2564'::uuid, 'de4ff6f0-ab03-5dce-a88e-d529310d0ae1'::uuid, 1, 13
WHERE NOT EXISTS (SELECT 1 FROM PriceTiers WHERE ProductId = 'de4ff6f0-ab03-5dce-a88e-d529310d0ae1'::uuid);
INSERT INTO ProductVariants (Id, ProductId, Size, Color, Sku, StockQuantity)
SELECT '68fe2646-ea3a-51a9-a124-5fb287807d7d'::uuid, 'de4ff6f0-ab03-5dce-a88e-d529310d0ae1'::uuid, '41-43', NULL, 'WEB-STATIC5-V1', 500
WHERE NOT EXISTS (SELECT 1 FROM ProductVariants WHERE Sku = 'WEB-STATIC5-V1');

-- static6: Adidas Cushioned (Черные)
INSERT INTO Products (Id, Name, CategoryId, BrandId, Description, Composition, BaseSku, WeightGrams, ImageUrlsJson, Rating, ReviewsCount, IsActive, MinPrice, AudienceTag, CreatedAt)
SELECT 'e5ea4f29-7f7d-55b3-a016-5a9579f3be38'::uuid, 'Adidas Cushioned (Черные)', 'ef45badd-483e-40a1-8040-00cde2f92550'::uuid, 'b16a0227-7999-5092-b4b3-569ac8a1320b'::uuid, 'Черные носки Adidas, усиленная пятка и мысок.', 'Хлопок 60%, Полиэстер 36%', 'WEB-STATIC6', 0, '["https://full-seller.com/assets/images/0%20%287%29.jpg"]'::text, 4.7, 0, TRUE, 13, 'male', now()
WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Id = 'e5ea4f29-7f7d-55b3-a016-5a9579f3be38'::uuid);
INSERT INTO PriceTiers (Id, ProductId, MinQuantity, PricePerUnit)
SELECT '5ce1be29-58c4-5146-b056-bc1f2eac713c'::uuid, 'e5ea4f29-7f7d-55b3-a016-5a9579f3be38'::uuid, 1, 13
WHERE NOT EXISTS (SELECT 1 FROM PriceTiers WHERE ProductId = 'e5ea4f29-7f7d-55b3-a016-5a9579f3be38'::uuid);
INSERT INTO ProductVariants (Id, ProductId, Size, Color, Sku, StockQuantity)
SELECT '70f9fd26-7f1c-5a4a-bc97-0838cf441f38'::uuid, 'e5ea4f29-7f7d-55b3-a016-5a9579f3be38'::uuid, '41-43', NULL, 'WEB-STATIC6-V1', 500
WHERE NOT EXISTS (SELECT 1 FROM ProductVariants WHERE Sku = 'WEB-STATIC6-V1');

-- static7: Adidas Cushioned (Серые)
INSERT INTO Products (Id, Name, CategoryId, BrandId, Description, Composition, BaseSku, WeightGrams, ImageUrlsJson, Rating, ReviewsCount, IsActive, MinPrice, AudienceTag, CreatedAt)
SELECT '7c9b7b4b-52e0-5d86-9523-b11d810ff037'::uuid, 'Adidas Cushioned (Серые)', 'ef45badd-483e-40a1-8040-00cde2f92550'::uuid, 'b16a0227-7999-5092-b4b3-569ac8a1320b'::uuid, 'Серые носки Adidas с амортизацией.', 'Хлопок 60%, Полиэстер 36%', 'WEB-STATIC7', 0, '["https://full-seller.com/assets/images/0%20%2813%29.jpg"]'::text, 4.4, 0, TRUE, 13, 'male', now()
WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Id = '7c9b7b4b-52e0-5d86-9523-b11d810ff037'::uuid);
INSERT INTO PriceTiers (Id, ProductId, MinQuantity, PricePerUnit)
SELECT '40ebadf5-052a-50c4-8ace-1e08a710c0fe'::uuid, '7c9b7b4b-52e0-5d86-9523-b11d810ff037'::uuid, 1, 13
WHERE NOT EXISTS (SELECT 1 FROM PriceTiers WHERE ProductId = '7c9b7b4b-52e0-5d86-9523-b11d810ff037'::uuid);
INSERT INTO ProductVariants (Id, ProductId, Size, Color, Sku, StockQuantity)
SELECT '16f5b5ae-e1bf-5857-a7ba-a961bb9e7d46'::uuid, '7c9b7b4b-52e0-5d86-9523-b11d810ff037'::uuid, '41-43', NULL, 'WEB-STATIC7-V1', 500
WHERE NOT EXISTS (SELECT 1 FROM ProductVariants WHERE Sku = 'WEB-STATIC7-V1');

-- static8: Adidas Cushioned (Синие)
INSERT INTO Products (Id, Name, CategoryId, BrandId, Description, Composition, BaseSku, WeightGrams, ImageUrlsJson, Rating, ReviewsCount, IsActive, MinPrice, AudienceTag, CreatedAt)
SELECT '0e76c43d-425d-5c1a-b438-95a91454ac0e'::uuid, 'Adidas Cushioned (Синие)', 'ef45badd-483e-40a1-8040-00cde2f92550'::uuid, 'b16a0227-7999-5092-b4b3-569ac8a1320b'::uuid, 'Синие носки Adidas с поддержкой свода стопы.', 'Хлопок 60%, Полиэстер 36%', 'WEB-STATIC8', 0, '["https://full-seller.com/assets/images/%D0%B0%D0%B4%D0%B4.png"]'::text, 4.5, 0, TRUE, 13, 'male', now()
WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Id = '0e76c43d-425d-5c1a-b438-95a91454ac0e'::uuid);
INSERT INTO PriceTiers (Id, ProductId, MinQuantity, PricePerUnit)
SELECT 'c2a1fe65-bcfa-5b23-8ddb-302103e44f2f'::uuid, '0e76c43d-425d-5c1a-b438-95a91454ac0e'::uuid, 1, 13
WHERE NOT EXISTS (SELECT 1 FROM PriceTiers WHERE ProductId = '0e76c43d-425d-5c1a-b438-95a91454ac0e'::uuid);
INSERT INTO ProductVariants (Id, ProductId, Size, Color, Sku, StockQuantity)
SELECT '1ae2bb44-dcf6-51b0-ba89-64f650ac9b16'::uuid, '0e76c43d-425d-5c1a-b438-95a91454ac0e'::uuid, '41-43', NULL, 'WEB-STATIC8-V1', 500
WHERE NOT EXISTS (SELECT 1 FROM ProductVariants WHERE Sku = 'WEB-STATIC8-V1');

-- static9: Puma Performance (Белые)
INSERT INTO Products (Id, Name, CategoryId, BrandId, Description, Composition, BaseSku, WeightGrams, ImageUrlsJson, Rating, ReviewsCount, IsActive, MinPrice, AudienceTag, CreatedAt)
SELECT '0418df04-d60a-5d15-a0c6-f05f8bfd5c59'::uuid, 'Puma Performance (Белые)', 'ef45badd-483e-40a1-8040-00cde2f92550'::uuid, 'fa6c347d-9db7-5f6e-bece-54c7afaffc8e'::uuid, 'Спортивные белые носки Puma.', 'Хлопок 78%, Полиамид 18%', 'WEB-STATIC9', 0, '["https://full-seller.com/assets/images/24.jpg"]'::text, 4.3, 0, TRUE, 13, 'male', now()
WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Id = '0418df04-d60a-5d15-a0c6-f05f8bfd5c59'::uuid);
INSERT INTO PriceTiers (Id, ProductId, MinQuantity, PricePerUnit)
SELECT '4264894c-6f8e-503d-86a2-2bdac268701a'::uuid, '0418df04-d60a-5d15-a0c6-f05f8bfd5c59'::uuid, 1, 13
WHERE NOT EXISTS (SELECT 1 FROM PriceTiers WHERE ProductId = '0418df04-d60a-5d15-a0c6-f05f8bfd5c59'::uuid);
INSERT INTO ProductVariants (Id, ProductId, Size, Color, Sku, StockQuantity)
SELECT 'ff77eed1-5abb-5bbe-9286-6a8ae8366afd'::uuid, '0418df04-d60a-5d15-a0c6-f05f8bfd5c59'::uuid, '41-43', NULL, 'WEB-STATIC9-V1', 500
WHERE NOT EXISTS (SELECT 1 FROM ProductVariants WHERE Sku = 'WEB-STATIC9-V1');

-- static10: Puma Performance (Черные)
INSERT INTO Products (Id, Name, CategoryId, BrandId, Description, Composition, BaseSku, WeightGrams, ImageUrlsJson, Rating, ReviewsCount, IsActive, MinPrice, AudienceTag, CreatedAt)
SELECT 'dcc9fd64-ad39-5e1c-9ffd-cf23a1734227'::uuid, 'Puma Performance (Черные)', 'ef45badd-483e-40a1-8040-00cde2f92550'::uuid, 'fa6c347d-9db7-5f6e-bece-54c7afaffc8e'::uuid, 'Черные носки Puma для тренировок и отдыха.', 'Хлопок 78%, Полиамид 18%', 'WEB-STATIC10', 0, '["https://full-seller.com/assets/images/25.jpg"]'::text, 4.6, 0, TRUE, 13, 'male', now()
WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Id = 'dcc9fd64-ad39-5e1c-9ffd-cf23a1734227'::uuid);
INSERT INTO PriceTiers (Id, ProductId, MinQuantity, PricePerUnit)
SELECT 'fc2c73fb-1a70-582c-a463-0422b344c0f6'::uuid, 'dcc9fd64-ad39-5e1c-9ffd-cf23a1734227'::uuid, 1, 13
WHERE NOT EXISTS (SELECT 1 FROM PriceTiers WHERE ProductId = 'dcc9fd64-ad39-5e1c-9ffd-cf23a1734227'::uuid);
INSERT INTO ProductVariants (Id, ProductId, Size, Color, Sku, StockQuantity)
SELECT 'c0c59605-9666-5e77-9d9f-43e020f11267'::uuid, 'dcc9fd64-ad39-5e1c-9ffd-cf23a1734227'::uuid, '41-43', NULL, 'WEB-STATIC10-V1', 500
WHERE NOT EXISTS (SELECT 1 FROM ProductVariants WHERE Sku = 'WEB-STATIC10-V1');

-- static11: Puma Performance (Серые)
INSERT INTO Products (Id, Name, CategoryId, BrandId, Description, Composition, BaseSku, WeightGrams, ImageUrlsJson, Rating, ReviewsCount, IsActive, MinPrice, AudienceTag, CreatedAt)
SELECT '09c157f6-d2ed-5a64-bcb2-fb8ddc7f312b'::uuid, 'Puma Performance (Серые)', 'ef45badd-483e-40a1-8040-00cde2f92550'::uuid, 'fa6c347d-9db7-5f6e-bece-54c7afaffc8e'::uuid, 'Серые носки Puma, дышащая сетка.', 'Хлопок 78%, Полиамид 18%', 'WEB-STATIC11', 0, '["https://full-seller.com/assets/images/9.jpg"]'::text, 4.4, 0, TRUE, 13, 'male', now()
WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Id = '09c157f6-d2ed-5a64-bcb2-fb8ddc7f312b'::uuid);
INSERT INTO PriceTiers (Id, ProductId, MinQuantity, PricePerUnit)
SELECT 'f093d5a5-5730-5d38-b2e1-3116730a6885'::uuid, '09c157f6-d2ed-5a64-bcb2-fb8ddc7f312b'::uuid, 1, 13
WHERE NOT EXISTS (SELECT 1 FROM PriceTiers WHERE ProductId = '09c157f6-d2ed-5a64-bcb2-fb8ddc7f312b'::uuid);
INSERT INTO ProductVariants (Id, ProductId, Size, Color, Sku, StockQuantity)
SELECT '1b31c26d-f0d7-5abc-a991-6f6790a15948'::uuid, '09c157f6-d2ed-5a64-bcb2-fb8ddc7f312b'::uuid, '41-43', NULL, 'WEB-STATIC11-V1', 500
WHERE NOT EXISTS (SELECT 1 FROM ProductVariants WHERE Sku = 'WEB-STATIC11-V1');

-- static12: Puma Performance (Синие)
INSERT INTO Products (Id, Name, CategoryId, BrandId, Description, Composition, BaseSku, WeightGrams, ImageUrlsJson, Rating, ReviewsCount, IsActive, MinPrice, AudienceTag, CreatedAt)
SELECT 'b974fa71-ba74-5fbb-9807-d38dd7f2ddbb'::uuid, 'Puma Performance (Синие)', 'ef45badd-483e-40a1-8040-00cde2f92550'::uuid, 'fa6c347d-9db7-5f6e-bece-54c7afaffc8e'::uuid, 'Ярко-синие носки Puma с отводом влаги.', 'Хлопок 78%, Полиамид 18%', 'WEB-STATIC12', 0, '["https://full-seller.com/assets/images/pmm.png"]'::text, 4.2, 0, TRUE, 13, 'male', now()
WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Id = 'b974fa71-ba74-5fbb-9807-d38dd7f2ddbb'::uuid);
INSERT INTO PriceTiers (Id, ProductId, MinQuantity, PricePerUnit)
SELECT '65202771-34d3-5409-8157-62c047eb05e3'::uuid, 'b974fa71-ba74-5fbb-9807-d38dd7f2ddbb'::uuid, 1, 13
WHERE NOT EXISTS (SELECT 1 FROM PriceTiers WHERE ProductId = 'b974fa71-ba74-5fbb-9807-d38dd7f2ddbb'::uuid);
INSERT INTO ProductVariants (Id, ProductId, Size, Color, Sku, StockQuantity)
SELECT 'e1b9902d-8295-531c-b06f-e3b1aeaa7820'::uuid, 'b974fa71-ba74-5fbb-9807-d38dd7f2ddbb'::uuid, '41-43', NULL, 'WEB-STATIC12-V1', 500
WHERE NOT EXISTS (SELECT 1 FROM ProductVariants WHERE Sku = 'WEB-STATIC12-V1');

-- static13: Fila Retro (Белые)
INSERT INTO Products (Id, Name, CategoryId, BrandId, Description, Composition, BaseSku, WeightGrams, ImageUrlsJson, Rating, ReviewsCount, IsActive, MinPrice, AudienceTag, CreatedAt)
SELECT '236578ff-3754-567c-9ce4-5d3e5f9221c5'::uuid, 'Fila Retro (Белые)', 'ef45badd-483e-40a1-8040-00cde2f92550'::uuid, 'c8cf75eb-12d0-58be-9220-03e4613110bf'::uuid, 'Белые носки Fila в ретро-стиле.', 'Хлопок 80%, Эластан 5%', 'WEB-STATIC13', 0, '["https://full-seller.com/assets/images/30.png"]'::text, 4.1, 0, TRUE, 13, 'male', now()
WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Id = '236578ff-3754-567c-9ce4-5d3e5f9221c5'::uuid);
INSERT INTO PriceTiers (Id, ProductId, MinQuantity, PricePerUnit)
SELECT 'db48be42-c23e-59a7-8861-7c0ade747cb0'::uuid, '236578ff-3754-567c-9ce4-5d3e5f9221c5'::uuid, 1, 13
WHERE NOT EXISTS (SELECT 1 FROM PriceTiers WHERE ProductId = '236578ff-3754-567c-9ce4-5d3e5f9221c5'::uuid);
INSERT INTO ProductVariants (Id, ProductId, Size, Color, Sku, StockQuantity)
SELECT 'b79ba7ea-d9fe-583d-96e8-09b46842134d'::uuid, '236578ff-3754-567c-9ce4-5d3e5f9221c5'::uuid, '37-39', NULL, 'WEB-STATIC13-V1', 500
WHERE NOT EXISTS (SELECT 1 FROM ProductVariants WHERE Sku = 'WEB-STATIC13-V1');

-- static14: Fila Retro (Черные)
INSERT INTO Products (Id, Name, CategoryId, BrandId, Description, Composition, BaseSku, WeightGrams, ImageUrlsJson, Rating, ReviewsCount, IsActive, MinPrice, AudienceTag, CreatedAt)
SELECT '75ebf775-baff-53f9-aae3-703eb6c1d3d3'::uuid, 'Fila Retro (Черные)', 'ef45badd-483e-40a1-8040-00cde2f92550'::uuid, 'c8cf75eb-12d0-58be-9220-03e4613110bf'::uuid, 'Черные носки Fila.', 'Хлопок 80%, Эластан 5%', 'WEB-STATIC14', 0, '["https://full-seller.com/assets/images/27.jpg"]'::text, 4.5, 0, TRUE, 13, 'male', now()
WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Id = '75ebf775-baff-53f9-aae3-703eb6c1d3d3'::uuid);
INSERT INTO PriceTiers (Id, ProductId, MinQuantity, PricePerUnit)
SELECT 'df381bbb-a337-5ede-8242-d78ae3e17905'::uuid, '75ebf775-baff-53f9-aae3-703eb6c1d3d3'::uuid, 1, 13
WHERE NOT EXISTS (SELECT 1 FROM PriceTiers WHERE ProductId = '75ebf775-baff-53f9-aae3-703eb6c1d3d3'::uuid);
INSERT INTO ProductVariants (Id, ProductId, Size, Color, Sku, StockQuantity)
SELECT '31d83793-b7cd-5870-b00c-934be70b02f1'::uuid, '75ebf775-baff-53f9-aae3-703eb6c1d3d3'::uuid, '37-39', NULL, 'WEB-STATIC14-V1', 500
WHERE NOT EXISTS (SELECT 1 FROM ProductVariants WHERE Sku = 'WEB-STATIC14-V1');

-- static15: Fila Retro (Серые)
INSERT INTO Products (Id, Name, CategoryId, BrandId, Description, Composition, BaseSku, WeightGrams, ImageUrlsJson, Rating, ReviewsCount, IsActive, MinPrice, AudienceTag, CreatedAt)
SELECT '40d64cfa-a06b-561e-976d-a614ee028c43'::uuid, 'Fila Retro (Серые)', 'ef45badd-483e-40a1-8040-00cde2f92550'::uuid, 'c8cf75eb-12d0-58be-9220-03e4613110bf'::uuid, 'Серые ретро-носки Fila.', 'Хлопок 80%, Эластан 5%', 'WEB-STATIC15', 0, '["https://full-seller.com/assets/images/5206550087578883063.jpg"]'::text, 4.3, 0, TRUE, 13, 'male', now()
WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Id = '40d64cfa-a06b-561e-976d-a614ee028c43'::uuid);
INSERT INTO PriceTiers (Id, ProductId, MinQuantity, PricePerUnit)
SELECT '47b5f792-73ed-54f9-a33b-919b49f0d8cd'::uuid, '40d64cfa-a06b-561e-976d-a614ee028c43'::uuid, 1, 13
WHERE NOT EXISTS (SELECT 1 FROM PriceTiers WHERE ProductId = '40d64cfa-a06b-561e-976d-a614ee028c43'::uuid);
INSERT INTO ProductVariants (Id, ProductId, Size, Color, Sku, StockQuantity)
SELECT '9a7b99f4-be9a-5553-ad24-40c7c7edd61b'::uuid, '40d64cfa-a06b-561e-976d-a614ee028c43'::uuid, '37-39', NULL, 'WEB-STATIC15-V1', 500
WHERE NOT EXISTS (SELECT 1 FROM ProductVariants WHERE Sku = 'WEB-STATIC15-V1');

-- static16: Fila Retro (Синие)
INSERT INTO Products (Id, Name, CategoryId, BrandId, Description, Composition, BaseSku, WeightGrams, ImageUrlsJson, Rating, ReviewsCount, IsActive, MinPrice, AudienceTag, CreatedAt)
SELECT 'c2616882-ded9-55c9-8380-c95faa6d7977'::uuid, 'Fila Retro (Синие)', 'ef45badd-483e-40a1-8040-00cde2f92550'::uuid, 'c8cf75eb-12d0-58be-9220-03e4613110bf'::uuid, 'Синие носки Fila.', 'Хлопок 80%, Эластан 5%', 'WEB-STATIC16', 0, '["https://full-seller.com/assets/images/5206550087578883063.jpg"]'::text, 4.4, 0, TRUE, 13, 'male', now()
WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Id = 'c2616882-ded9-55c9-8380-c95faa6d7977'::uuid);
INSERT INTO PriceTiers (Id, ProductId, MinQuantity, PricePerUnit)
SELECT 'c59ece41-1150-5fdd-a1b9-964f930d6349'::uuid, 'c2616882-ded9-55c9-8380-c95faa6d7977'::uuid, 1, 13
WHERE NOT EXISTS (SELECT 1 FROM PriceTiers WHERE ProductId = 'c2616882-ded9-55c9-8380-c95faa6d7977'::uuid);
INSERT INTO ProductVariants (Id, ProductId, Size, Color, Sku, StockQuantity)
SELECT '25e012b1-87f9-54d9-ae66-932a36ec87b1'::uuid, 'c2616882-ded9-55c9-8380-c95faa6d7977'::uuid, '37-39', NULL, 'WEB-STATIC16-V1', 500
WHERE NOT EXISTS (SELECT 1 FROM ProductVariants WHERE Sku = 'WEB-STATIC16-V1');

-- regular1: Обычные носки (Белые)
INSERT INTO Products (Id, Name, CategoryId, BrandId, Description, Composition, BaseSku, WeightGrams, ImageUrlsJson, Rating, ReviewsCount, IsActive, MinPrice, AudienceTag, CreatedAt)
SELECT 'f90cf87d-aa10-5217-9a78-8d4557a59b8c'::uuid, 'Обычные носки (Белые)', 'ef45badd-483e-40a1-8040-00cde2f92550'::uuid, '36bc313e-13c3-5412-9330-722533ef58b7'::uuid, 'Классические белые носки.', 'Хлопок 80%', 'WEB-REGULAR1', 0, '["https://full-seller.com/assets/images/80.jpg"]'::text, 4.0, 0, TRUE, 13, 'male', now()
WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Id = 'f90cf87d-aa10-5217-9a78-8d4557a59b8c'::uuid);
INSERT INTO PriceTiers (Id, ProductId, MinQuantity, PricePerUnit)
SELECT 'a7c8b1a0-7383-54dc-93b5-041bd2e4dbcf'::uuid, 'f90cf87d-aa10-5217-9a78-8d4557a59b8c'::uuid, 1, 13
WHERE NOT EXISTS (SELECT 1 FROM PriceTiers WHERE ProductId = 'f90cf87d-aa10-5217-9a78-8d4557a59b8c'::uuid);
INSERT INTO ProductVariants (Id, ProductId, Size, Color, Sku, StockQuantity)
SELECT '5deac8b3-38be-5ab8-8e8e-6118044caef1'::uuid, 'f90cf87d-aa10-5217-9a78-8d4557a59b8c'::uuid, '41-43', NULL, 'WEB-REGULAR1-V1', 500
WHERE NOT EXISTS (SELECT 1 FROM ProductVariants WHERE Sku = 'WEB-REGULAR1-V1');

-- regular2: Обычные носки (Черные)
INSERT INTO Products (Id, Name, CategoryId, BrandId, Description, Composition, BaseSku, WeightGrams, ImageUrlsJson, Rating, ReviewsCount, IsActive, MinPrice, AudienceTag, CreatedAt)
SELECT '3607afcf-d8d4-5b2b-b417-525195d51c34'::uuid, 'Обычные носки (Черные)', 'ef45badd-483e-40a1-8040-00cde2f92550'::uuid, '36bc313e-13c3-5412-9330-722533ef58b7'::uuid, 'Черные базовые носки.', 'Хлопок 80%', 'WEB-REGULAR2', 0, '["https://full-seller.com/assets/images/adbel.jpg"]'::text, 4.1, 0, TRUE, 13, 'male', now()
WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Id = '3607afcf-d8d4-5b2b-b417-525195d51c34'::uuid);
INSERT INTO PriceTiers (Id, ProductId, MinQuantity, PricePerUnit)
SELECT '913dbfe1-6bfd-5ecb-8e06-f57263657100'::uuid, '3607afcf-d8d4-5b2b-b417-525195d51c34'::uuid, 1, 13
WHERE NOT EXISTS (SELECT 1 FROM PriceTiers WHERE ProductId = '3607afcf-d8d4-5b2b-b417-525195d51c34'::uuid);
INSERT INTO ProductVariants (Id, ProductId, Size, Color, Sku, StockQuantity)
SELECT '39ce9c18-5fdd-5b94-b9ff-ff064a418d1f'::uuid, '3607afcf-d8d4-5b2b-b417-525195d51c34'::uuid, '41-43', NULL, 'WEB-REGULAR2-V1', 500
WHERE NOT EXISTS (SELECT 1 FROM ProductVariants WHERE Sku = 'WEB-REGULAR2-V1');

-- regular3: Обычные носки (Серые)
INSERT INTO Products (Id, Name, CategoryId, BrandId, Description, Composition, BaseSku, WeightGrams, ImageUrlsJson, Rating, ReviewsCount, IsActive, MinPrice, AudienceTag, CreatedAt)
SELECT '3abf9bba-2501-5463-8a5b-29d9e4036a7c'::uuid, 'Обычные носки (Серые)', 'ef45badd-483e-40a1-8040-00cde2f92550'::uuid, '36bc313e-13c3-5412-9330-722533ef58b7'::uuid, 'Серые носки средней длины.', 'Хлопок 80%', 'WEB-REGULAR3', 0, '["https://full-seller.com/assets/images/81.jpg"]'::text, 4.1, 0, TRUE, 13, 'male', now()
WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Id = '3abf9bba-2501-5463-8a5b-29d9e4036a7c'::uuid);
INSERT INTO PriceTiers (Id, ProductId, MinQuantity, PricePerUnit)
SELECT 'bd46827e-c622-52d1-b796-865df105c1c0'::uuid, '3abf9bba-2501-5463-8a5b-29d9e4036a7c'::uuid, 1, 13
WHERE NOT EXISTS (SELECT 1 FROM PriceTiers WHERE ProductId = '3abf9bba-2501-5463-8a5b-29d9e4036a7c'::uuid);
INSERT INTO ProductVariants (Id, ProductId, Size, Color, Sku, StockQuantity)
SELECT '3da83e26-fb99-5a0e-8cb3-a625b7ebbc14'::uuid, '3abf9bba-2501-5463-8a5b-29d9e4036a7c'::uuid, '37-39', NULL, 'WEB-REGULAR3-V1', 500
WHERE NOT EXISTS (SELECT 1 FROM ProductVariants WHERE Sku = 'WEB-REGULAR3-V1');

-- regular4: Обычные носки (Синие)
INSERT INTO Products (Id, Name, CategoryId, BrandId, Description, Composition, BaseSku, WeightGrams, ImageUrlsJson, Rating, ReviewsCount, IsActive, MinPrice, AudienceTag, CreatedAt)
SELECT 'dddbe4bd-ec01-5cd8-b15d-baf10d479e6f'::uuid, 'Обычные носки (Синие)', 'ef45badd-483e-40a1-8040-00cde2f92550'::uuid, '36bc313e-13c3-5412-9330-722533ef58b7'::uuid, 'Синие носки на каждый день.', 'Хлопок 80%', 'WEB-REGULAR4', 0, '["https://full-seller.com/assets/images/500.jpg"]'::text, 4.0, 0, TRUE, 13, 'male', now()
WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Id = 'dddbe4bd-ec01-5cd8-b15d-baf10d479e6f'::uuid);
INSERT INTO PriceTiers (Id, ProductId, MinQuantity, PricePerUnit)
SELECT '2285dd92-af8a-567c-8979-5b2a5a08f9e9'::uuid, 'dddbe4bd-ec01-5cd8-b15d-baf10d479e6f'::uuid, 1, 13
WHERE NOT EXISTS (SELECT 1 FROM PriceTiers WHERE ProductId = 'dddbe4bd-ec01-5cd8-b15d-baf10d479e6f'::uuid);
INSERT INTO ProductVariants (Id, ProductId, Size, Color, Sku, StockQuantity)
SELECT 'cc555cb6-55dc-5bb7-84da-a2695b2e6b50'::uuid, 'dddbe4bd-ec01-5cd8-b15d-baf10d479e6f'::uuid, '37-39', NULL, 'WEB-REGULAR4-V1', 500
WHERE NOT EXISTS (SELECT 1 FROM ProductVariants WHERE Sku = 'WEB-REGULAR4-V1');

-- tshirt1: Classic White (Белая)
INSERT INTO Products (Id, Name, CategoryId, BrandId, Description, Composition, BaseSku, WeightGrams, ImageUrlsJson, Rating, ReviewsCount, IsActive, MinPrice, AudienceTag, CreatedAt)
SELECT '0161b08a-d62b-54cb-b141-ea308d8f6c36'::uuid, 'Classic White (Белая)', '8ba474a6-2eda-43ad-adc0-749f0c980e8b'::uuid, 'ec2615c8-3fc0-5b89-9992-0c09878abfc6'::uuid, 'Классическая белая футболка.', 'Хлопок 100%', 'WEB-TSHIRT1', 0, '["https://full-seller.com/assets/images/90.jpg"]'::text, 4.6, 0, TRUE, 145, 'male', now()
WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Id = '0161b08a-d62b-54cb-b141-ea308d8f6c36'::uuid);
INSERT INTO PriceTiers (Id, ProductId, MinQuantity, PricePerUnit)
SELECT '44c888e0-dd47-59dc-806d-d73a28b2840c'::uuid, '0161b08a-d62b-54cb-b141-ea308d8f6c36'::uuid, 1, 145
WHERE NOT EXISTS (SELECT 1 FROM PriceTiers WHERE ProductId = '0161b08a-d62b-54cb-b141-ea308d8f6c36'::uuid);
INSERT INTO ProductVariants (Id, ProductId, Size, Color, Sku, StockQuantity)
SELECT 'cf6412e5-b75d-5cfe-a640-4343e3cda3f5'::uuid, '0161b08a-d62b-54cb-b141-ea308d8f6c36'::uuid, 'M', NULL, 'WEB-TSHIRT1-V1', 300
WHERE NOT EXISTS (SELECT 1 FROM ProductVariants WHERE Sku = 'WEB-TSHIRT1-V1');
INSERT INTO ProductVariants (Id, ProductId, Size, Color, Sku, StockQuantity)
SELECT '7eb4bf62-fc45-5aea-8a80-c950d738c94a'::uuid, '0161b08a-d62b-54cb-b141-ea308d8f6c36'::uuid, 'L', NULL, 'WEB-TSHIRT1-V2', 300
WHERE NOT EXISTS (SELECT 1 FROM ProductVariants WHERE Sku = 'WEB-TSHIRT1-V2');
INSERT INTO ProductVariants (Id, ProductId, Size, Color, Sku, StockQuantity)
SELECT 'f4daab3a-92c7-531d-8015-97e18a380480'::uuid, '0161b08a-d62b-54cb-b141-ea308d8f6c36'::uuid, 'XL (50-54)', NULL, 'WEB-TSHIRT1-V3', 300
WHERE NOT EXISTS (SELECT 1 FROM ProductVariants WHERE Sku = 'WEB-TSHIRT1-V3');

-- tshirt4: Basic Dark (Бордовая)
INSERT INTO Products (Id, Name, CategoryId, BrandId, Description, Composition, BaseSku, WeightGrams, ImageUrlsJson, Rating, ReviewsCount, IsActive, MinPrice, AudienceTag, CreatedAt)
SELECT '0e4d370c-b232-5946-ab65-e1ac44ed58cf'::uuid, 'Basic Dark (Бордовая)', '8ba474a6-2eda-43ad-adc0-749f0c980e8b'::uuid, 'b16a0227-7999-5092-b4b3-569ac8a1320b'::uuid, 'Однотонная бордовая футболка.', 'Хлопок 100%', 'WEB-TSHIRT4', 0, '["https://full-seller.com/assets/images/91.jpg"]'::text, 4.6, 0, TRUE, 145, 'male', now()
WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Id = '0e4d370c-b232-5946-ab65-e1ac44ed58cf'::uuid);
INSERT INTO PriceTiers (Id, ProductId, MinQuantity, PricePerUnit)
SELECT '70eca10b-3eb9-5230-bcd5-af6a6c1228be'::uuid, '0e4d370c-b232-5946-ab65-e1ac44ed58cf'::uuid, 1, 145
WHERE NOT EXISTS (SELECT 1 FROM PriceTiers WHERE ProductId = '0e4d370c-b232-5946-ab65-e1ac44ed58cf'::uuid);
INSERT INTO ProductVariants (Id, ProductId, Size, Color, Sku, StockQuantity)
SELECT 'f27f4f25-12d4-58b1-9082-a8103be89d6b'::uuid, '0e4d370c-b232-5946-ab65-e1ac44ed58cf'::uuid, 'S', NULL, 'WEB-TSHIRT4-V1', 300
WHERE NOT EXISTS (SELECT 1 FROM ProductVariants WHERE Sku = 'WEB-TSHIRT4-V1');
INSERT INTO ProductVariants (Id, ProductId, Size, Color, Sku, StockQuantity)
SELECT '259bfa39-c204-530a-9927-7549ce1991bd'::uuid, '0e4d370c-b232-5946-ab65-e1ac44ed58cf'::uuid, 'M', NULL, 'WEB-TSHIRT4-V2', 300
WHERE NOT EXISTS (SELECT 1 FROM ProductVariants WHERE Sku = 'WEB-TSHIRT4-V2');
INSERT INTO ProductVariants (Id, ProductId, Size, Color, Sku, StockQuantity)
SELECT '3731547c-2ed1-54fe-b15d-50da9c7899a2'::uuid, '0e4d370c-b232-5946-ab65-e1ac44ed58cf'::uuid, 'L (44-48)', NULL, 'WEB-TSHIRT4-V3', 300
WHERE NOT EXISTS (SELECT 1 FROM ProductVariants WHERE Sku = 'WEB-TSHIRT4-V3');

-- tshirt3: Basic Light (Белая)
INSERT INTO Products (Id, Name, CategoryId, BrandId, Description, Composition, BaseSku, WeightGrams, ImageUrlsJson, Rating, ReviewsCount, IsActive, MinPrice, AudienceTag, CreatedAt)
SELECT 'b5807f6b-1a8e-5f65-925e-41442c55f704'::uuid, 'Basic Light (Белая)', '8ba474a6-2eda-43ad-adc0-749f0c980e8b'::uuid, 'b16a0227-7999-5092-b4b3-569ac8a1320b'::uuid, 'Базовая светлая футболка.', 'Хлопок 100%', 'WEB-TSHIRT3', 0, '["https://full-seller.com/assets/images/92.jpg"]'::text, 4.5, 0, TRUE, 145, 'male', now()
WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Id = 'b5807f6b-1a8e-5f65-925e-41442c55f704'::uuid);
INSERT INTO PriceTiers (Id, ProductId, MinQuantity, PricePerUnit)
SELECT '492e4ab7-f95b-5f65-9054-3088e56b20bd'::uuid, 'b5807f6b-1a8e-5f65-925e-41442c55f704'::uuid, 1, 145
WHERE NOT EXISTS (SELECT 1 FROM PriceTiers WHERE ProductId = 'b5807f6b-1a8e-5f65-925e-41442c55f704'::uuid);
INSERT INTO ProductVariants (Id, ProductId, Size, Color, Sku, StockQuantity)
SELECT 'b4ca654e-0d45-51f4-a052-ef334b753408'::uuid, 'b5807f6b-1a8e-5f65-925e-41442c55f704'::uuid, 'S', NULL, 'WEB-TSHIRT3-V1', 300
WHERE NOT EXISTS (SELECT 1 FROM ProductVariants WHERE Sku = 'WEB-TSHIRT3-V1');
INSERT INTO ProductVariants (Id, ProductId, Size, Color, Sku, StockQuantity)
SELECT '3ceeb48a-d8ac-58cf-99da-83a61d61aba3'::uuid, 'b5807f6b-1a8e-5f65-925e-41442c55f704'::uuid, 'M', NULL, 'WEB-TSHIRT3-V2', 300
WHERE NOT EXISTS (SELECT 1 FROM ProductVariants WHERE Sku = 'WEB-TSHIRT3-V2');
INSERT INTO ProductVariants (Id, ProductId, Size, Color, Sku, StockQuantity)
SELECT 'fe0fb2a6-4d7a-5ce4-bdd3-d4ec12b7a91f'::uuid, 'b5807f6b-1a8e-5f65-925e-41442c55f704'::uuid, 'L (44-48)', NULL, 'WEB-TSHIRT3-V3', 300
WHERE NOT EXISTS (SELECT 1 FROM ProductVariants WHERE Sku = 'WEB-TSHIRT3-V3');

-- tshirt2: Urban Black (Черная)
INSERT INTO Products (Id, Name, CategoryId, BrandId, Description, Composition, BaseSku, WeightGrams, ImageUrlsJson, Rating, ReviewsCount, IsActive, MinPrice, AudienceTag, CreatedAt)
SELECT '6a6e25eb-d5f5-523e-a4d2-648fc06f4d57'::uuid, 'Urban Black (Черная)', '8ba474a6-2eda-43ad-adc0-749f0c980e8b'::uuid, 'ec2615c8-3fc0-5b89-9992-0c09878abfc6'::uuid, 'Черная футболка в городском стиле.', 'Хлопок 100%', 'WEB-TSHIRT2', 0, '["https://full-seller.com/assets/images/91.jpg"]'::text, 4.7, 0, TRUE, 145, 'male', now()
WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Id = '6a6e25eb-d5f5-523e-a4d2-648fc06f4d57'::uuid);
INSERT INTO PriceTiers (Id, ProductId, MinQuantity, PricePerUnit)
SELECT 'c784c3ca-97ff-5156-a312-97c681185c5f'::uuid, '6a6e25eb-d5f5-523e-a4d2-648fc06f4d57'::uuid, 1, 145
WHERE NOT EXISTS (SELECT 1 FROM PriceTiers WHERE ProductId = '6a6e25eb-d5f5-523e-a4d2-648fc06f4d57'::uuid);
INSERT INTO ProductVariants (Id, ProductId, Size, Color, Sku, StockQuantity)
SELECT '057f44da-9cc8-5da7-9358-98be83627f32'::uuid, '6a6e25eb-d5f5-523e-a4d2-648fc06f4d57'::uuid, 'M', NULL, 'WEB-TSHIRT2-V1', 300
WHERE NOT EXISTS (SELECT 1 FROM ProductVariants WHERE Sku = 'WEB-TSHIRT2-V1');
INSERT INTO ProductVariants (Id, ProductId, Size, Color, Sku, StockQuantity)
SELECT '224cc33e-89fd-54dc-99d3-8ea49ffcaeed'::uuid, '6a6e25eb-d5f5-523e-a4d2-648fc06f4d57'::uuid, 'L', NULL, 'WEB-TSHIRT2-V2', 300
WHERE NOT EXISTS (SELECT 1 FROM ProductVariants WHERE Sku = 'WEB-TSHIRT2-V2');
INSERT INTO ProductVariants (Id, ProductId, Size, Color, Sku, StockQuantity)
SELECT '4ea37d2c-e169-56b4-b4b6-57a51ce683c3'::uuid, '6a6e25eb-d5f5-523e-a4d2-648fc06f4d57'::uuid, 'XL (50-54)', NULL, 'WEB-TSHIRT2-V3', 300
WHERE NOT EXISTS (SELECT 1 FROM ProductVariants WHERE Sku = 'WEB-TSHIRT2-V3');

-- tshirt5: Vintage Washed (Графит)
INSERT INTO Products (Id, Name, CategoryId, BrandId, Description, Composition, BaseSku, WeightGrams, ImageUrlsJson, Rating, ReviewsCount, IsActive, MinPrice, AudienceTag, CreatedAt)
SELECT '46b1bce0-64f2-5385-a3d0-05204ce855bc'::uuid, 'Vintage Washed (Графит)', '8ba474a6-2eda-43ad-adc0-749f0c980e8b'::uuid, 'fa6c347d-9db7-5f6e-bece-54c7afaffc8e'::uuid, 'Оверсайз футболка.', 'Хлопок 100%', 'WEB-TSHIRT5', 0, '["https://full-seller.com/assets/images/92.jpg"]'::text, 4.4, 0, TRUE, 145, 'male', now()
WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Id = '46b1bce0-64f2-5385-a3d0-05204ce855bc'::uuid);
INSERT INTO PriceTiers (Id, ProductId, MinQuantity, PricePerUnit)
SELECT 'a9665714-c6ca-5a06-b80e-c58b7d30d3a0'::uuid, '46b1bce0-64f2-5385-a3d0-05204ce855bc'::uuid, 1, 145
WHERE NOT EXISTS (SELECT 1 FROM PriceTiers WHERE ProductId = '46b1bce0-64f2-5385-a3d0-05204ce855bc'::uuid);
INSERT INTO ProductVariants (Id, ProductId, Size, Color, Sku, StockQuantity)
SELECT '459fe3cf-45a7-5aa9-92d8-8c31432e7cb6'::uuid, '46b1bce0-64f2-5385-a3d0-05204ce855bc'::uuid, 'M', NULL, 'WEB-TSHIRT5-V1', 300
WHERE NOT EXISTS (SELECT 1 FROM ProductVariants WHERE Sku = 'WEB-TSHIRT5-V1');
INSERT INTO ProductVariants (Id, ProductId, Size, Color, Sku, StockQuantity)
SELECT '70ef574a-5a48-5233-9116-0deb7b21def0'::uuid, '46b1bce0-64f2-5385-a3d0-05204ce855bc'::uuid, 'L', NULL, 'WEB-TSHIRT5-V2', 300
WHERE NOT EXISTS (SELECT 1 FROM ProductVariants WHERE Sku = 'WEB-TSHIRT5-V2');
INSERT INTO ProductVariants (Id, ProductId, Size, Color, Sku, StockQuantity)
SELECT '34ca39ef-7a8e-5626-98e1-51cb10f9ef2f'::uuid, '46b1bce0-64f2-5385-a3d0-05204ce855bc'::uuid, 'XL (50-54)', NULL, 'WEB-TSHIRT5-V3', 300
WHERE NOT EXISTS (SELECT 1 FROM ProductVariants WHERE Sku = 'WEB-TSHIRT5-V3');

-- tshirt6: Summer Tank (Белая)
INSERT INTO Products (Id, Name, CategoryId, BrandId, Description, Composition, BaseSku, WeightGrams, ImageUrlsJson, Rating, ReviewsCount, IsActive, MinPrice, AudienceTag, CreatedAt)
SELECT 'bc8b7860-3bc9-5a10-a9cf-22d3dc84bfa3'::uuid, 'Summer Tank (Белая)', '8ba474a6-2eda-43ad-adc0-749f0c980e8b'::uuid, 'fa6c347d-9db7-5f6e-bece-54c7afaffc8e'::uuid, 'Легкая белая майка.', 'Хлопок 100%', 'WEB-TSHIRT6', 0, '["https://full-seller.com/assets/images/95.jpg"]'::text, 4.3, 0, TRUE, 145, 'male', now()
WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Id = 'bc8b7860-3bc9-5a10-a9cf-22d3dc84bfa3'::uuid);
INSERT INTO PriceTiers (Id, ProductId, MinQuantity, PricePerUnit)
SELECT 'f58f5a4f-391c-59f6-bb0c-ae8794ac2a29'::uuid, 'bc8b7860-3bc9-5a10-a9cf-22d3dc84bfa3'::uuid, 1, 145
WHERE NOT EXISTS (SELECT 1 FROM PriceTiers WHERE ProductId = 'bc8b7860-3bc9-5a10-a9cf-22d3dc84bfa3'::uuid);
INSERT INTO ProductVariants (Id, ProductId, Size, Color, Sku, StockQuantity)
SELECT '93383100-dde3-55c4-9520-d06e2c15955d'::uuid, 'bc8b7860-3bc9-5a10-a9cf-22d3dc84bfa3'::uuid, 'S', NULL, 'WEB-TSHIRT6-V1', 300
WHERE NOT EXISTS (SELECT 1 FROM ProductVariants WHERE Sku = 'WEB-TSHIRT6-V1');
INSERT INTO ProductVariants (Id, ProductId, Size, Color, Sku, StockQuantity)
SELECT 'ce90d0b4-6534-5c8f-9547-6b0f3de7ffd5'::uuid, 'bc8b7860-3bc9-5a10-a9cf-22d3dc84bfa3'::uuid, 'M', NULL, 'WEB-TSHIRT6-V2', 300
WHERE NOT EXISTS (SELECT 1 FROM ProductVariants WHERE Sku = 'WEB-TSHIRT6-V2');
INSERT INTO ProductVariants (Id, ProductId, Size, Color, Sku, StockQuantity)
SELECT '1598e67e-d449-5331-8894-c54dc2c59f72'::uuid, 'bc8b7860-3bc9-5a10-a9cf-22d3dc84bfa3'::uuid, 'L (44-48)', NULL, 'WEB-TSHIRT6-V3', 300
WHERE NOT EXISTS (SELECT 1 FROM ProductVariants WHERE Sku = 'WEB-TSHIRT6-V3');

-- tshirt7: Футболка мужская (Белая)
INSERT INTO Products (Id, Name, CategoryId, BrandId, Description, Composition, BaseSku, WeightGrams, ImageUrlsJson, Rating, ReviewsCount, IsActive, MinPrice, AudienceTag, CreatedAt)
SELECT 'bb8040b0-b5ed-593c-8a95-dc300bff8dca'::uuid, 'Футболка мужская (Белая)', '8ba474a6-2eda-43ad-adc0-749f0c980e8b'::uuid, '36bc313e-13c3-5412-9330-722533ef58b7'::uuid, 'Базовая футболка прямого кроя.', 'Хлопок 100%', 'WEB-TSHIRT7', 0, '["https://full-seller.com/assets/images/90.jpg"]'::text, 4.8, 0, TRUE, 145, 'male', now()
WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Id = 'bb8040b0-b5ed-593c-8a95-dc300bff8dca'::uuid);
INSERT INTO PriceTiers (Id, ProductId, MinQuantity, PricePerUnit)
SELECT '191ad1d3-50f9-5cb4-9ad6-0551d273cfc2'::uuid, 'bb8040b0-b5ed-593c-8a95-dc300bff8dca'::uuid, 1, 145
WHERE NOT EXISTS (SELECT 1 FROM PriceTiers WHERE ProductId = 'bb8040b0-b5ed-593c-8a95-dc300bff8dca'::uuid);
INSERT INTO ProductVariants (Id, ProductId, Size, Color, Sku, StockQuantity)
SELECT 'da3ac299-b7ee-5771-96d7-473ac00503f1'::uuid, 'bb8040b0-b5ed-593c-8a95-dc300bff8dca'::uuid, 'M', NULL, 'WEB-TSHIRT7-V1', 200
WHERE NOT EXISTS (SELECT 1 FROM ProductVariants WHERE Sku = 'WEB-TSHIRT7-V1');
INSERT INTO ProductVariants (Id, ProductId, Size, Color, Sku, StockQuantity)
SELECT 'ea109bdc-a5e3-594d-a97e-b821c3a3f7f8'::uuid, 'bb8040b0-b5ed-593c-8a95-dc300bff8dca'::uuid, 'L', NULL, 'WEB-TSHIRT7-V2', 200
WHERE NOT EXISTS (SELECT 1 FROM ProductVariants WHERE Sku = 'WEB-TSHIRT7-V2');
INSERT INTO ProductVariants (Id, ProductId, Size, Color, Sku, StockQuantity)
SELECT 'd7543ca3-3f27-5941-b557-d0355e356ec8'::uuid, 'bb8040b0-b5ed-593c-8a95-dc300bff8dca'::uuid, 'XL (50-54)', NULL, 'WEB-TSHIRT7-V3', 200
WHERE NOT EXISTS (SELECT 1 FROM ProductVariants WHERE Sku = 'WEB-TSHIRT7-V3');

-- tshirt8: Футболка мужская (Черная)
INSERT INTO Products (Id, Name, CategoryId, BrandId, Description, Composition, BaseSku, WeightGrams, ImageUrlsJson, Rating, ReviewsCount, IsActive, MinPrice, AudienceTag, CreatedAt)
SELECT '87f3dbe2-5c37-5356-b82c-44bb9dee97f2'::uuid, 'Футболка мужская (Черная)', '8ba474a6-2eda-43ad-adc0-749f0c980e8b'::uuid, '36bc313e-13c3-5412-9330-722533ef58b7'::uuid, 'Классическая однотонная футболка.', 'Хлопок 100%', 'WEB-TSHIRT8', 0, '["https://full-seller.com/assets/images/91.jpg"]'::text, 4.7, 0, TRUE, 145, 'male', now()
WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Id = '87f3dbe2-5c37-5356-b82c-44bb9dee97f2'::uuid);
INSERT INTO PriceTiers (Id, ProductId, MinQuantity, PricePerUnit)
SELECT 'd7d1ec00-1884-5d39-bb11-69fcdec9fdbb'::uuid, '87f3dbe2-5c37-5356-b82c-44bb9dee97f2'::uuid, 1, 145
WHERE NOT EXISTS (SELECT 1 FROM PriceTiers WHERE ProductId = '87f3dbe2-5c37-5356-b82c-44bb9dee97f2'::uuid);
INSERT INTO ProductVariants (Id, ProductId, Size, Color, Sku, StockQuantity)
SELECT 'c8b65346-c885-5c63-afc4-491832d5c8d1'::uuid, '87f3dbe2-5c37-5356-b82c-44bb9dee97f2'::uuid, 'M', NULL, 'WEB-TSHIRT8-V1', 250
WHERE NOT EXISTS (SELECT 1 FROM ProductVariants WHERE Sku = 'WEB-TSHIRT8-V1');
INSERT INTO ProductVariants (Id, ProductId, Size, Color, Sku, StockQuantity)
SELECT '96cca509-e8dd-5f34-8a41-7a4d12d69d62'::uuid, '87f3dbe2-5c37-5356-b82c-44bb9dee97f2'::uuid, 'L', NULL, 'WEB-TSHIRT8-V2', 250
WHERE NOT EXISTS (SELECT 1 FROM ProductVariants WHERE Sku = 'WEB-TSHIRT8-V2');
INSERT INTO ProductVariants (Id, ProductId, Size, Color, Sku, StockQuantity)
SELECT '091cbf9b-1826-5343-b586-20572d0289fd'::uuid, '87f3dbe2-5c37-5356-b82c-44bb9dee97f2'::uuid, 'XL (50-54)', NULL, 'WEB-TSHIRT8-V3', 250
WHERE NOT EXISTS (SELECT 1 FROM ProductVariants WHERE Sku = 'WEB-TSHIRT8-V3');

-- tshirt9: Футболка мужская (Синяя)
INSERT INTO Products (Id, Name, CategoryId, BrandId, Description, Composition, BaseSku, WeightGrams, ImageUrlsJson, Rating, ReviewsCount, IsActive, MinPrice, AudienceTag, CreatedAt)
SELECT 'ba8b4f98-62f0-53ea-9323-9f7fc37c7cd6'::uuid, 'Футболка мужская (Синяя)', '8ba474a6-2eda-43ad-adc0-749f0c980e8b'::uuid, '36bc313e-13c3-5412-9330-722533ef58b7'::uuid, 'Стильная футболка темно-синего оттенка.', 'Хлопок 100%', 'WEB-TSHIRT9', 0, '["https://full-seller.com/assets/images/92.jpg"]'::text, 4.6, 0, TRUE, 145, 'male', now()
WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Id = 'ba8b4f98-62f0-53ea-9323-9f7fc37c7cd6'::uuid);
INSERT INTO PriceTiers (Id, ProductId, MinQuantity, PricePerUnit)
SELECT 'abeb8e98-a36e-5220-a79f-4d05cd9de344'::uuid, 'ba8b4f98-62f0-53ea-9323-9f7fc37c7cd6'::uuid, 1, 145
WHERE NOT EXISTS (SELECT 1 FROM PriceTiers WHERE ProductId = 'ba8b4f98-62f0-53ea-9323-9f7fc37c7cd6'::uuid);
INSERT INTO ProductVariants (Id, ProductId, Size, Color, Sku, StockQuantity)
SELECT '4c032f65-e4eb-5a3f-8ce5-a99c464650f2'::uuid, 'ba8b4f98-62f0-53ea-9323-9f7fc37c7cd6'::uuid, 'M', NULL, 'WEB-TSHIRT9-V1', 150
WHERE NOT EXISTS (SELECT 1 FROM ProductVariants WHERE Sku = 'WEB-TSHIRT9-V1');
INSERT INTO ProductVariants (Id, ProductId, Size, Color, Sku, StockQuantity)
SELECT '814fdec4-cf80-5e95-8b19-57649666447b'::uuid, 'ba8b4f98-62f0-53ea-9323-9f7fc37c7cd6'::uuid, 'L', NULL, 'WEB-TSHIRT9-V2', 150
WHERE NOT EXISTS (SELECT 1 FROM ProductVariants WHERE Sku = 'WEB-TSHIRT9-V2');
INSERT INTO ProductVariants (Id, ProductId, Size, Color, Sku, StockQuantity)
SELECT '5cc9675a-c5ad-5e26-b7a2-0e33d5eb85eb'::uuid, 'ba8b4f98-62f0-53ea-9323-9f7fc37c7cd6'::uuid, 'XL (50-54)', NULL, 'WEB-TSHIRT9-V3', 150
WHERE NOT EXISTS (SELECT 1 FROM ProductVariants WHERE Sku = 'WEB-TSHIRT9-V3');

-- uw1: Nike Boxer (Белые)
INSERT INTO Products (Id, Name, CategoryId, BrandId, Description, Composition, BaseSku, WeightGrams, ImageUrlsJson, Rating, ReviewsCount, IsActive, MinPrice, AudienceTag, CreatedAt)
SELECT 'f995ff5f-e5b2-5b44-b86d-d4b120ad3fe2'::uuid, 'Nike Boxer (Белые)', 'c277d19f-a1d4-4658-b153-9b0eb01b808d'::uuid, 'ec2615c8-3fc0-5b89-9992-0c09878abfc6'::uuid, 'Белые боксеры Nike Dri-FIT.', 'Хлопок 95%, Эластан 5%', 'WEB-UW1', 0, '["https://full-seller.com/assets/images/110.png"]'::text, 4.5, 0, TRUE, 145, 'male', now()
WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Id = 'f995ff5f-e5b2-5b44-b86d-d4b120ad3fe2'::uuid);
INSERT INTO PriceTiers (Id, ProductId, MinQuantity, PricePerUnit)
SELECT '4c8de0ed-2ea5-5e0a-8db3-bc7627e417a5'::uuid, 'f995ff5f-e5b2-5b44-b86d-d4b120ad3fe2'::uuid, 1, 145
WHERE NOT EXISTS (SELECT 1 FROM PriceTiers WHERE ProductId = 'f995ff5f-e5b2-5b44-b86d-d4b120ad3fe2'::uuid);
INSERT INTO ProductVariants (Id, ProductId, Size, Color, Sku, StockQuantity)
SELECT '86f71b2a-ce1f-55bc-99b2-db9c91c8f267'::uuid, 'f995ff5f-e5b2-5b44-b86d-d4b120ad3fe2'::uuid, 'M', NULL, 'WEB-UW1-V1', 400
WHERE NOT EXISTS (SELECT 1 FROM ProductVariants WHERE Sku = 'WEB-UW1-V1');
INSERT INTO ProductVariants (Id, ProductId, Size, Color, Sku, StockQuantity)
SELECT 'a05c24ed-9adb-5565-a272-c6eaddd56651'::uuid, 'f995ff5f-e5b2-5b44-b86d-d4b120ad3fe2'::uuid, 'L', NULL, 'WEB-UW1-V2', 400
WHERE NOT EXISTS (SELECT 1 FROM ProductVariants WHERE Sku = 'WEB-UW1-V2');
INSERT INTO ProductVariants (Id, ProductId, Size, Color, Sku, StockQuantity)
SELECT '81cc27e8-71b6-58ba-a072-32a3a464f544'::uuid, 'f995ff5f-e5b2-5b44-b86d-d4b120ad3fe2'::uuid, 'XL (50-54)', NULL, 'WEB-UW1-V3', 400
WHERE NOT EXISTS (SELECT 1 FROM ProductVariants WHERE Sku = 'WEB-UW1-V3');

-- uw2: Nike Boxer (Черные)
INSERT INTO Products (Id, Name, CategoryId, BrandId, Description, Composition, BaseSku, WeightGrams, ImageUrlsJson, Rating, ReviewsCount, IsActive, MinPrice, AudienceTag, CreatedAt)
SELECT 'ab35e516-00e8-5d2f-8b07-76569f538323'::uuid, 'Nike Boxer (Черные)', 'c277d19f-a1d4-4658-b153-9b0eb01b808d'::uuid, 'ec2615c8-3fc0-5b89-9992-0c09878abfc6'::uuid, 'Черные боксеры Nike.', 'Хлопок 95%, Эластан 5%', 'WEB-UW2', 0, '["https://full-seller.com/assets/images/110.png"]'::text, 4.6, 0, TRUE, 145, 'male', now()
WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Id = 'ab35e516-00e8-5d2f-8b07-76569f538323'::uuid);
INSERT INTO PriceTiers (Id, ProductId, MinQuantity, PricePerUnit)
SELECT '32d9df75-778b-5b75-a5a4-c2af58044201'::uuid, 'ab35e516-00e8-5d2f-8b07-76569f538323'::uuid, 1, 145
WHERE NOT EXISTS (SELECT 1 FROM PriceTiers WHERE ProductId = 'ab35e516-00e8-5d2f-8b07-76569f538323'::uuid);
INSERT INTO ProductVariants (Id, ProductId, Size, Color, Sku, StockQuantity)
SELECT '23c98a22-c3d4-5289-b7a3-e6298a51d53e'::uuid, 'ab35e516-00e8-5d2f-8b07-76569f538323'::uuid, 'M', NULL, 'WEB-UW2-V1', 400
WHERE NOT EXISTS (SELECT 1 FROM ProductVariants WHERE Sku = 'WEB-UW2-V1');
INSERT INTO ProductVariants (Id, ProductId, Size, Color, Sku, StockQuantity)
SELECT 'b846e5c4-cdb6-5088-95ee-b29419171213'::uuid, 'ab35e516-00e8-5d2f-8b07-76569f538323'::uuid, 'L', NULL, 'WEB-UW2-V2', 400
WHERE NOT EXISTS (SELECT 1 FROM ProductVariants WHERE Sku = 'WEB-UW2-V2');
INSERT INTO ProductVariants (Id, ProductId, Size, Color, Sku, StockQuantity)
SELECT 'ddd5308e-5070-5922-b5cc-b982505536df'::uuid, 'ab35e516-00e8-5d2f-8b07-76569f538323'::uuid, 'XL (50-54)', NULL, 'WEB-UW2-V3', 400
WHERE NOT EXISTS (SELECT 1 FROM ProductVariants WHERE Sku = 'WEB-UW2-V3');

-- pants1: Брюки-джоггеры камуфляжные
INSERT INTO Products (Id, Name, CategoryId, BrandId, Description, Composition, BaseSku, WeightGrams, ImageUrlsJson, Rating, ReviewsCount, IsActive, MinPrice, AudienceTag, CreatedAt)
SELECT '26726821-7bb4-50f1-bbc7-160ac428ecd8'::uuid, 'Брюки-джоггеры камуфляжные', '631d76fa-6f65-4ee1-90fe-5848eedb9095'::uuid, '36bc313e-13c3-5412-9330-722533ef58b7'::uuid, 'Повседневные тактические штаны.', 'Хлопок 65%, Полиэстер 35%', 'WEB-PANTS1', 0, '["https://full-seller.com/assets/images/459.jpg"]'::text, 4.9, 0, TRUE, 145, 'male', now()
WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Id = '26726821-7bb4-50f1-bbc7-160ac428ecd8'::uuid);
INSERT INTO PriceTiers (Id, ProductId, MinQuantity, PricePerUnit)
SELECT 'ca74b770-2bde-5de6-80cc-c072a92ffcc4'::uuid, '26726821-7bb4-50f1-bbc7-160ac428ecd8'::uuid, 1, 145
WHERE NOT EXISTS (SELECT 1 FROM PriceTiers WHERE ProductId = '26726821-7bb4-50f1-bbc7-160ac428ecd8'::uuid);
INSERT INTO ProductVariants (Id, ProductId, Size, Color, Sku, StockQuantity)
SELECT '457b06ac-c84d-5b67-b064-4d22b70048fb'::uuid, '26726821-7bb4-50f1-bbc7-160ac428ecd8'::uuid, 'M', NULL, 'WEB-PANTS1-V1', 80
WHERE NOT EXISTS (SELECT 1 FROM ProductVariants WHERE Sku = 'WEB-PANTS1-V1');
INSERT INTO ProductVariants (Id, ProductId, Size, Color, Sku, StockQuantity)
SELECT 'e7764ee2-4e30-541d-8060-79e7fdce77e6'::uuid, '26726821-7bb4-50f1-bbc7-160ac428ecd8'::uuid, 'L', NULL, 'WEB-PANTS1-V2', 80
WHERE NOT EXISTS (SELECT 1 FROM ProductVariants WHERE Sku = 'WEB-PANTS1-V2');
INSERT INTO ProductVariants (Id, ProductId, Size, Color, Sku, StockQuantity)
SELECT 'aadaccf0-97f7-5a82-a722-31abd87e3144'::uuid, '26726821-7bb4-50f1-bbc7-160ac428ecd8'::uuid, 'XL (50-54)', NULL, 'WEB-PANTS1-V3', 80
WHERE NOT EXISTS (SELECT 1 FROM ProductVariants WHERE Sku = 'WEB-PANTS1-V3');

-- shorts1: Шорты мужские трикотажные
INSERT INTO Products (Id, Name, CategoryId, BrandId, Description, Composition, BaseSku, WeightGrams, ImageUrlsJson, Rating, ReviewsCount, IsActive, MinPrice, AudienceTag, CreatedAt)
SELECT 'bd6fae0a-8932-52ca-85b0-1f0020d1b985'::uuid, 'Шорты мужские трикотажные', 'fce656b0-a4c5-4cc6-8100-73725f1554ff'::uuid, '36bc313e-13c3-5412-9330-722533ef58b7'::uuid, 'Удлиненные летние шорты.', 'Хлопок 80%, Полиэстер 20%', 'WEB-SHORTS1', 0, '["https://full-seller.com/assets/images/459.jpg"]'::text, 4.5, 0, TRUE, 145, 'male', now()
WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Id = 'bd6fae0a-8932-52ca-85b0-1f0020d1b985'::uuid);
INSERT INTO PriceTiers (Id, ProductId, MinQuantity, PricePerUnit)
SELECT '4cabcb86-a1cc-5db9-a70c-bebaa2a4758b'::uuid, 'bd6fae0a-8932-52ca-85b0-1f0020d1b985'::uuid, 1, 145
WHERE NOT EXISTS (SELECT 1 FROM PriceTiers WHERE ProductId = 'bd6fae0a-8932-52ca-85b0-1f0020d1b985'::uuid);
INSERT INTO ProductVariants (Id, ProductId, Size, Color, Sku, StockQuantity)
SELECT 'ea5772c0-bc43-59fc-8d37-a5c8ed72c900'::uuid, 'bd6fae0a-8932-52ca-85b0-1f0020d1b985'::uuid, 'M', NULL, 'WEB-SHORTS1-V1', 120
WHERE NOT EXISTS (SELECT 1 FROM ProductVariants WHERE Sku = 'WEB-SHORTS1-V1');
INSERT INTO ProductVariants (Id, ProductId, Size, Color, Sku, StockQuantity)
SELECT 'd87bde62-ec0f-5550-b169-b79668ad1c71'::uuid, 'bd6fae0a-8932-52ca-85b0-1f0020d1b985'::uuid, 'L', NULL, 'WEB-SHORTS1-V2', 120
WHERE NOT EXISTS (SELECT 1 FROM ProductVariants WHERE Sku = 'WEB-SHORTS1-V2');
INSERT INTO ProductVariants (Id, ProductId, Size, Color, Sku, StockQuantity)
SELECT '66165847-e53f-50e2-b65e-8ee00f6c7642'::uuid, 'bd6fae0a-8932-52ca-85b0-1f0020d1b985'::uuid, 'XL (50-54)', NULL, 'WEB-SHORTS1-V3', 120
WHERE NOT EXISTS (SELECT 1 FROM ProductVariants WHERE Sku = 'WEB-SHORTS1-V3');

