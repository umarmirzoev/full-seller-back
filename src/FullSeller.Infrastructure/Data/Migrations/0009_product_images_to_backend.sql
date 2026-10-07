-- Фото товаров теперь отдаются самим бэкендом (wwwroot/media/products), а не сайтом full-seller.com
-- Заменяем ссылки в ImageUrlsJson (текстовая колонка).
UPDATE Products SET ImageUrlsJson = REPLACE(ImageUrlsJson, 'https://full-seller.com/assets/images/%D0%B0%D0%B4%D0%B4.png', 'https://api.full-seller.com/media/products/p01.jpg');
UPDATE Products SET ImageUrlsJson = REPLACE(ImageUrlsJson, 'https://full-seller.com/assets/images/0%20%2812%29.jpg', 'https://api.full-seller.com/media/products/p02.jpg');
UPDATE Products SET ImageUrlsJson = REPLACE(ImageUrlsJson, 'https://full-seller.com/assets/images/0%20%2813%29.jpg', 'https://api.full-seller.com/media/products/p03.jpg');
UPDATE Products SET ImageUrlsJson = REPLACE(ImageUrlsJson, 'https://full-seller.com/assets/images/0%20%2819%29.jpg', 'https://api.full-seller.com/media/products/p04.jpg');
UPDATE Products SET ImageUrlsJson = REPLACE(ImageUrlsJson, 'https://full-seller.com/assets/images/0%20%284%29.jpg', 'https://api.full-seller.com/media/products/p05.jpg');
UPDATE Products SET ImageUrlsJson = REPLACE(ImageUrlsJson, 'https://full-seller.com/assets/images/0%20%285%29.jpg', 'https://api.full-seller.com/media/products/p06.jpg');
UPDATE Products SET ImageUrlsJson = REPLACE(ImageUrlsJson, 'https://full-seller.com/assets/images/0%20%286%29.jpg', 'https://api.full-seller.com/media/products/p07.jpg');
UPDATE Products SET ImageUrlsJson = REPLACE(ImageUrlsJson, 'https://full-seller.com/assets/images/0%20%287%29.jpg', 'https://api.full-seller.com/media/products/p08.jpg');
UPDATE Products SET ImageUrlsJson = REPLACE(ImageUrlsJson, 'https://full-seller.com/assets/images/110.png', 'https://api.full-seller.com/media/products/p09.jpg');
UPDATE Products SET ImageUrlsJson = REPLACE(ImageUrlsJson, 'https://full-seller.com/assets/images/24.jpg', 'https://api.full-seller.com/media/products/p10.jpg');
UPDATE Products SET ImageUrlsJson = REPLACE(ImageUrlsJson, 'https://full-seller.com/assets/images/25.jpg', 'https://api.full-seller.com/media/products/p11.jpg');
UPDATE Products SET ImageUrlsJson = REPLACE(ImageUrlsJson, 'https://full-seller.com/assets/images/27.jpg', 'https://api.full-seller.com/media/products/p12.jpg');
UPDATE Products SET ImageUrlsJson = REPLACE(ImageUrlsJson, 'https://full-seller.com/assets/images/30.png', 'https://api.full-seller.com/media/products/p13.jpg');
UPDATE Products SET ImageUrlsJson = REPLACE(ImageUrlsJson, 'https://full-seller.com/assets/images/459.jpg', 'https://api.full-seller.com/media/products/p14.jpg');
UPDATE Products SET ImageUrlsJson = REPLACE(ImageUrlsJson, 'https://full-seller.com/assets/images/500.jpg', 'https://api.full-seller.com/media/products/p15.jpg');
UPDATE Products SET ImageUrlsJson = REPLACE(ImageUrlsJson, 'https://full-seller.com/assets/images/5206550087578883063.jpg', 'https://api.full-seller.com/media/products/p16.jpg');
UPDATE Products SET ImageUrlsJson = REPLACE(ImageUrlsJson, 'https://full-seller.com/assets/images/80.jpg', 'https://api.full-seller.com/media/products/p17.jpg');
UPDATE Products SET ImageUrlsJson = REPLACE(ImageUrlsJson, 'https://full-seller.com/assets/images/81.jpg', 'https://api.full-seller.com/media/products/p18.jpg');
UPDATE Products SET ImageUrlsJson = REPLACE(ImageUrlsJson, 'https://full-seller.com/assets/images/9.jpg', 'https://api.full-seller.com/media/products/p19.jpg');
UPDATE Products SET ImageUrlsJson = REPLACE(ImageUrlsJson, 'https://full-seller.com/assets/images/90.jpg', 'https://api.full-seller.com/media/products/p20.jpg');
UPDATE Products SET ImageUrlsJson = REPLACE(ImageUrlsJson, 'https://full-seller.com/assets/images/91.jpg', 'https://api.full-seller.com/media/products/p21.jpg');
UPDATE Products SET ImageUrlsJson = REPLACE(ImageUrlsJson, 'https://full-seller.com/assets/images/92.jpg', 'https://api.full-seller.com/media/products/p22.jpg');
UPDATE Products SET ImageUrlsJson = REPLACE(ImageUrlsJson, 'https://full-seller.com/assets/images/95.jpg', 'https://api.full-seller.com/media/products/p23.jpg');
UPDATE Products SET ImageUrlsJson = REPLACE(ImageUrlsJson, 'https://full-seller.com/assets/images/adbel.jpg', 'https://api.full-seller.com/media/products/p24.jpg');
UPDATE Products SET ImageUrlsJson = REPLACE(ImageUrlsJson, 'https://full-seller.com/assets/images/pmm.png', 'https://api.full-seller.com/media/products/p25.jpg');
