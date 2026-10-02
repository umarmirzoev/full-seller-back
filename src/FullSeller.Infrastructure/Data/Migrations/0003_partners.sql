-- FULL SELLER — партнёры (производители/оптовые поставщики) с отдельным входом по логину и паролю
-- (выдаётся вручную администратором — не через клиентский OTP-флоу, см. Users/OtpCodes в 0001_init.sql).
-- Применяется автоматически при старте (см. MigrationRunner), после 0002_settings_banners_product_badges.sql.
-- Ничего в существующих таблицах не удаляется и не переименовывается — только добавления.

CREATE TABLE Partners (
    Id UUID NOT NULL PRIMARY KEY,
    CompanyName VARCHAR(300) NOT NULL,
    ContactName VARCHAR(200) NOT NULL,
    Phone VARCHAR(20) NOT NULL,
    Login VARCHAR(100) NOT NULL,
    PasswordHash VARCHAR(500) NOT NULL,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE,
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT now(),
    CONSTRAINT UQ_Partners_Login UNIQUE (Login)
);

CREATE TABLE PartnerRefreshTokens (
    Id UUID NOT NULL PRIMARY KEY,
    PartnerId UUID NOT NULL REFERENCES Partners(Id),
    TokenHash VARCHAR(200) NOT NULL,
    ExpiresAt TIMESTAMPTZ NOT NULL,
    RevokedAt TIMESTAMPTZ NULL,
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT now()
);
CREATE INDEX IX_PartnerRefreshTokens_PartnerId_ExpiresAt ON PartnerRefreshTokens(PartnerId, ExpiresAt);

-- Товары, добавленные партнёром через его личный кабинет (api/partner/products) — NULL для всех
-- существующих товаров и для товаров, добавляемых из обычной админ-панели, так что ничего не ломается.
ALTER TABLE Products ADD COLUMN PartnerId UUID NULL REFERENCES Partners(Id);
CREATE INDEX IX_Products_PartnerId ON Products(PartnerId);
