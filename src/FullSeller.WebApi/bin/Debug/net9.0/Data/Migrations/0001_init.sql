-- FULL SELLER — начальная схема БД (PostgreSQL)
-- Применяется по порядку номеров файлов (0001_, 0002_, ...) простым раннером при старте приложения.
-- Идентификаторы не квотируются нигде (ни здесь, ни в C#-коде репозиториев) — PostgreSQL
-- последовательно приводит все неквотированные имена к нижнему регистру, поэтому регистр
-- в этом файле и в SQL-запросах репозиториев может не совпадать буква в букву — это нормально.

CREATE TABLE Users (
    Id UUID NOT NULL PRIMARY KEY,
    Phone VARCHAR(20) NOT NULL,
    FullName VARCHAR(200) NULL,
    Role INT NOT NULL DEFAULT 0,
    IsLegalEntity BOOLEAN NOT NULL DEFAULT FALSE,
    LegalName VARCHAR(300) NULL,
    TaxId VARCHAR(50) NULL,
    LoyaltyPoints INT NOT NULL DEFAULT 0,
    Language VARCHAR(10) NOT NULL DEFAULT 'ru',
    PreferredCurrency VARCHAR(10) NOT NULL DEFAULT 'RUB',
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT now(),
    CONSTRAINT UQ_Users_Phone UNIQUE (Phone)
);

CREATE TABLE Addresses (
    Id UUID NOT NULL PRIMARY KEY,
    UserId UUID NOT NULL REFERENCES Users(Id),
    Country INT NOT NULL,
    City VARCHAR(100) NOT NULL,
    Line VARCHAR(300) NOT NULL,
    IsDefault BOOLEAN NOT NULL DEFAULT FALSE
);
CREATE INDEX IX_Addresses_UserId ON Addresses(UserId);

CREATE TABLE OtpCodes (
    Id UUID NOT NULL PRIMARY KEY,
    Phone VARCHAR(20) NOT NULL,
    CodeHash VARCHAR(200) NOT NULL,
    ExpiresAt TIMESTAMPTZ NOT NULL,
    Attempts INT NOT NULL DEFAULT 0,
    IsUsed BOOLEAN NOT NULL DEFAULT FALSE,
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT now()
);
CREATE INDEX IX_OtpCodes_Phone_ExpiresAt ON OtpCodes(Phone, ExpiresAt);

CREATE TABLE RefreshTokens (
    Id UUID NOT NULL PRIMARY KEY,
    UserId UUID NOT NULL REFERENCES Users(Id),
    TokenHash VARCHAR(200) NOT NULL,
    ExpiresAt TIMESTAMPTZ NOT NULL,
    RevokedAt TIMESTAMPTZ NULL,
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT now()
);
CREATE INDEX IX_RefreshTokens_UserId_ExpiresAt ON RefreshTokens(UserId, ExpiresAt);

CREATE TABLE Categories (
    Id UUID NOT NULL PRIMARY KEY,
    Name VARCHAR(150) NOT NULL,
    Slug VARCHAR(150) NOT NULL,
    ParentCategoryId UUID NULL REFERENCES Categories(Id)
);

CREATE TABLE Brands (
    Id UUID NOT NULL PRIMARY KEY,
    Name VARCHAR(150) NOT NULL,
    LogoUrl VARCHAR(500) NULL
);

CREATE TABLE Products (
    Id UUID NOT NULL PRIMARY KEY,
    Name VARCHAR(300) NOT NULL,
    CategoryId UUID NOT NULL REFERENCES Categories(Id),
    BrandId UUID NULL REFERENCES Brands(Id),
    Description TEXT NULL,
    Composition VARCHAR(500) NULL,
    BaseSku VARCHAR(50) NOT NULL,
    WeightGrams INT NOT NULL DEFAULT 0,
    ImageUrlsJson TEXT NOT NULL DEFAULT '[]',
    Rating DECIMAL(3,2) NOT NULL DEFAULT 0,
    ReviewsCount INT NOT NULL DEFAULT 0,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE,
    MinPrice DECIMAL(10,2) NOT NULL DEFAULT 0,
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT now()
);
CREATE INDEX IX_Products_CategoryId ON Products(CategoryId);
CREATE INDEX IX_Products_BrandId ON Products(BrandId);

CREATE TABLE ProductVariants (
    Id UUID NOT NULL PRIMARY KEY,
    ProductId UUID NOT NULL REFERENCES Products(Id),
    Size VARCHAR(20) NULL,
    Color VARCHAR(50) NULL,
    Sku VARCHAR(50) NOT NULL,
    StockQuantity INT NOT NULL DEFAULT 0,
    CONSTRAINT UQ_ProductVariants_Sku UNIQUE (Sku)
);
CREATE INDEX IX_ProductVariants_ProductId ON ProductVariants(ProductId);

CREATE TABLE PriceTiers (
    Id UUID NOT NULL PRIMARY KEY,
    ProductId UUID NOT NULL REFERENCES Products(Id),
    MinQuantity INT NOT NULL,
    PricePerUnit DECIMAL(10,2) NOT NULL
);
CREATE INDEX IX_PriceTiers_ProductId_MinQuantity ON PriceTiers(ProductId, MinQuantity);

CREATE TABLE Carts (
    Id UUID NOT NULL PRIMARY KEY,
    UserId UUID NOT NULL REFERENCES Users(Id),
    PromoCodeId UUID NULL,
    CONSTRAINT UQ_Carts_UserId UNIQUE (UserId)
);

CREATE TABLE CartItems (
    Id UUID NOT NULL PRIMARY KEY,
    CartId UUID NOT NULL REFERENCES Carts(Id),
    ProductVariantId UUID NOT NULL REFERENCES ProductVariants(Id),
    Quantity INT NOT NULL
);
CREATE INDEX IX_CartItems_CartId ON CartItems(CartId);

CREATE TABLE Orders (
    Id UUID NOT NULL PRIMARY KEY,
    OrderNumber VARCHAR(30) NOT NULL,
    UserId UUID NOT NULL REFERENCES Users(Id),
    Status INT NOT NULL DEFAULT 0,
    DeliveryCountry INT NOT NULL,
    AddressId UUID NOT NULL REFERENCES Addresses(Id),
    PaymentMethod INT NOT NULL,
    TotalAmount DECIMAL(12,2) NOT NULL,
    DeliveryCost DECIMAL(10,2) NOT NULL DEFAULT 0,
    CargoTrackingNumber VARCHAR(100) NULL,
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT now(),
    CONSTRAINT UQ_Orders_OrderNumber UNIQUE (OrderNumber)
);
CREATE INDEX IX_Orders_UserId ON Orders(UserId);
CREATE INDEX IX_Orders_Status ON Orders(Status);

CREATE TABLE OrderItems (
    Id UUID NOT NULL PRIMARY KEY,
    OrderId UUID NOT NULL REFERENCES Orders(Id),
    ProductVariantId UUID NOT NULL REFERENCES ProductVariants(Id),
    Quantity INT NOT NULL,
    UnitPriceAtOrderTime DECIMAL(10,2) NOT NULL
);
CREATE INDEX IX_OrderItems_OrderId ON OrderItems(OrderId);

CREATE TABLE OrderStatusEvents (
    Id UUID NOT NULL PRIMARY KEY,
    OrderId UUID NOT NULL REFERENCES Orders(Id),
    Status INT NOT NULL,
    Comment VARCHAR(500) NULL,
    ChangedByUserId UUID NULL,
    ChangedAt TIMESTAMPTZ NOT NULL DEFAULT now()
);
CREATE INDEX IX_OrderStatusEvents_OrderId_ChangedAt ON OrderStatusEvents(OrderId, ChangedAt);

CREATE TABLE Reviews (
    Id UUID NOT NULL PRIMARY KEY,
    ProductId UUID NOT NULL REFERENCES Products(Id),
    UserId UUID NOT NULL REFERENCES Users(Id),
    OrderId UUID NOT NULL REFERENCES Orders(Id),
    Rating INT NOT NULL,
    Text TEXT NULL,
    PhotoUrlsJson TEXT NOT NULL DEFAULT '[]',
    Status INT NOT NULL DEFAULT 0,
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT now()
);
CREATE INDEX IX_Reviews_ProductId ON Reviews(ProductId);
CREATE INDEX IX_Reviews_Status ON Reviews(Status);

CREATE TABLE Favorites (
    Id UUID NOT NULL PRIMARY KEY,
    UserId UUID NOT NULL REFERENCES Users(Id),
    ProductId UUID NOT NULL REFERENCES Products(Id),
    CONSTRAINT UQ_Favorites_User_Product UNIQUE (UserId, ProductId)
);

CREATE TABLE ChatMessages (
    Id UUID NOT NULL PRIMARY KEY,
    UserId UUID NOT NULL REFERENCES Users(Id),
    SenderType INT NOT NULL,
    Text TEXT NOT NULL,
    AttachmentUrl VARCHAR(500) NULL,
    SentAt TIMESTAMPTZ NOT NULL DEFAULT now(),
    IsRead BOOLEAN NOT NULL DEFAULT FALSE
);
CREATE INDEX IX_ChatMessages_UserId_SentAt ON ChatMessages(UserId, SentAt);

CREATE TABLE Notifications (
    Id UUID NOT NULL PRIMARY KEY,
    UserId UUID NOT NULL REFERENCES Users(Id),
    Type INT NOT NULL,
    Title VARCHAR(200) NOT NULL,
    Body VARCHAR(1000) NOT NULL,
    IsRead BOOLEAN NOT NULL DEFAULT FALSE,
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT now()
);
CREATE INDEX IX_Notifications_UserId_IsRead ON Notifications(UserId, IsRead);

CREATE TABLE PromoCodes (
    Id UUID NOT NULL PRIMARY KEY,
    Code VARCHAR(50) NOT NULL,
    DiscountType INT NOT NULL,
    DiscountValue DECIMAL(10,2) NOT NULL,
    ExpiresAt TIMESTAMPTZ NULL,
    UsageLimit INT NULL,
    TimesUsed INT NOT NULL DEFAULT 0,
    CONSTRAINT UQ_PromoCodes_Code UNIQUE (Code)
);

CREATE TABLE CargoRates (
    Id UUID NOT NULL PRIMARY KEY,
    Country INT NOT NULL,
    PricePerKg DECIMAL(10,2) NOT NULL,
    Currency VARCHAR(10) NOT NULL DEFAULT 'RUB',
    CONSTRAINT UQ_CargoRates_Country UNIQUE (Country)
);

-- Стартовые тарифы карго (можно поменять из админки в будущем).
-- gen_random_uuid() встроена в PostgreSQL начиная с версии 13 — расширение pgcrypto не требуется.
INSERT INTO CargoRates (Id, Country, PricePerKg, Currency) VALUES
    (gen_random_uuid(), 0, 350.00, 'RUB'),  -- Russia
    (gen_random_uuid(), 1, 60.00, 'TJS');   -- Tajikistan
