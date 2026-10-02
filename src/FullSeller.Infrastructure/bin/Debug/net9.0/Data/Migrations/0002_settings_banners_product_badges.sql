-- FULL SELLER 2.0 — добавляет то, чего не хватало в первой версии бэкенда по ТЗ:
--   • Products: старая цена (для бейджа «Скидка») и ручные метки «Новинка»/«Хит»;
--   • AppSettings: курс доллара (с датой обновления) и минимальная сумма заказа — ТЗ п.8 и п.10;
--   • Banners: баннеры главной страницы, редактируемые из админ-панели — ТЗ п.3 и п.10.
-- Применяется автоматически при старте (см. MigrationRunner), после 0001_init.sql.

ALTER TABLE Products ADD COLUMN OldPrice DECIMAL(10,2) NULL;
ALTER TABLE Products ADD COLUMN IsNew BOOLEAN NOT NULL DEFAULT FALSE;
ALTER TABLE Products ADD COLUMN IsHit BOOLEAN NOT NULL DEFAULT FALSE;

CREATE TABLE AppSettings (
    Id UUID NOT NULL PRIMARY KEY,
    UsdToRubRate DECIMAL(10,4) NOT NULL DEFAULT 90,
    RateUpdatedAt TIMESTAMPTZ NOT NULL DEFAULT now(),
    MinOrderAmount DECIMAL(10,2) NOT NULL DEFAULT 2000,
    ContactPhone VARCHAR(30) NULL,
    ContactAddress VARCHAR(300) NULL,
    WhatsAppUrl VARCHAR(300) NULL,
    TelegramUrl VARCHAR(300) NULL,
    UpdatedAt TIMESTAMPTZ NOT NULL DEFAULT now()
);

-- Единственная строка настроек магазина (таблица-синглтон — репозиторий всегда читает/пишет первую строку).
INSERT INTO AppSettings (Id, UsdToRubRate, RateUpdatedAt, MinOrderAmount, ContactPhone, ContactAddress, WhatsAppUrl, TelegramUrl, UpdatedAt)
VALUES (gen_random_uuid(), 90.00, now(), 2000.00, '+992901234567', 'Душанбе, Сино р-н, ул. Рудаки 45', 'https://wa.me/992901234567', 'https://t.me/fullseller', now());

CREATE TABLE Banners (
    Id UUID NOT NULL PRIMARY KEY,
    Title VARCHAR(200) NOT NULL,
    Subtitle VARCHAR(300) NULL,
    ImageUrl VARCHAR(500) NULL,
    LinkUrl VARCHAR(500) NULL,
    SortOrder INT NOT NULL DEFAULT 0,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE,
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT now()
);
CREATE INDEX IX_Banners_IsActive_SortOrder ON Banners(IsActive, SortOrder);

INSERT INTO Banners (Id, Title, Subtitle, SortOrder, IsActive, CreatedAt) VALUES
    (gen_random_uuid(), 'Новая поставка Nike Everyday', 'До 15% дешевле при заказе от 5000 пар. Успей до конца недели.', 0, TRUE, now());
