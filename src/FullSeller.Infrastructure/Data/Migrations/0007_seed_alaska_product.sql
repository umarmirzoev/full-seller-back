-- 0007_seed_alaska_product.sql
-- Товар «Аляска/Alaska термоноски зимные» (был добавлен на сайте через Firebase, не входил в статический сид 0005).
-- Добавляем его в реальные таблицы, чтобы он отображался и в Flutter-приложении. Идемпотентно.

INSERT INTO Brands (Id, Name, LogoUrl)
SELECT '21bdd7e2-0dac-530b-87b6-d31722b806e5'::uuid, 'Alaska', NULL
WHERE NOT EXISTS (SELECT 1 FROM Brands WHERE Name = 'Alaska');

INSERT INTO Products (Id, Name, CategoryId, BrandId, Description, Composition, BaseSku, WeightGrams, ImageUrlsJson, Rating, ReviewsCount, IsActive, MinPrice, AudienceTag, CreatedAt)
SELECT '45e61de4-6872-5a88-bfac-f4e5c9910ec2'::uuid,
       'Аляска/Alaska термоноски зимные',
       'ef45badd-483e-40a1-8040-00cde2f92550'::uuid,
       (SELECT Id FROM Brands WHERE Name = 'Alaska' LIMIT 1),
       'Аляска/Alaska термоноски зимные — носки от проверенного производителя из Узбекистана. Модель сочетает практичный дизайн и комфортную посадку на каждый день. Подходит для оптовых закупок и розничной продажи, доступны разные размеры и цвета.',
       NULL, 'WEB-ALASKA1', 70,
       '["https://i.ibb.co/v44JmY1G/de56e2cf238b.jpg","https://i.ibb.co/b52QMS7V/2ca2d7e55a2d.jpg","https://i.ibb.co/kVcTTxwb/40ae8b890c5c.jpg"]'::text,
       4.2, 0, TRUE, 30.8, 'male', now()
WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Id = '45e61de4-6872-5a88-bfac-f4e5c9910ec2'::uuid);

INSERT INTO PriceTiers (Id, ProductId, MinQuantity, PricePerUnit)
SELECT 'aa9c4cc5-8c63-562b-880d-394333343c4f'::uuid, '45e61de4-6872-5a88-bfac-f4e5c9910ec2'::uuid, 1, 30.8
WHERE NOT EXISTS (SELECT 1 FROM PriceTiers WHERE ProductId = '45e61de4-6872-5a88-bfac-f4e5c9910ec2'::uuid);

INSERT INTO ProductVariants (Id, ProductId, Size, Color, Sku, StockQuantity)
SELECT '8b7b8cf2-5303-5dc3-bea3-973329aa86f5'::uuid, '45e61de4-6872-5a88-bfac-f4e5c9910ec2'::uuid, '41-43', NULL, 'WEB-ALASKA1-V1', 500
WHERE NOT EXISTS (SELECT 1 FROM ProductVariants WHERE Sku = 'WEB-ALASKA1-V1');

-- На сайте «Fila Retro (Черные)» из статического списка не показывается (перекрыт записью Firestore с тем же id),
-- поэтому прячем его и в приложении, чтобы каталог совпадал с сайтом (33 товара + «РЕКЛАМА» без цены).
UPDATE Products SET IsActive = FALSE WHERE Name = 'Fila Retro (Черные)';
