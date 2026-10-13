-- Контакты, адрес-снимок, комментарий и канал уведомления в заказе (для админ-панели и автоотправки в мессенджер).
ALTER TABLE Orders ADD COLUMN IF NOT EXISTS ContactName VARCHAR(200) NULL;
ALTER TABLE Orders ADD COLUMN IF NOT EXISTS ContactPhone VARCHAR(50) NULL;
ALTER TABLE Orders ADD COLUMN IF NOT EXISTS AddressText VARCHAR(500) NULL;
ALTER TABLE Orders ADD COLUMN IF NOT EXISTS Comment VARCHAR(1000) NULL;
ALTER TABLE Orders ADD COLUMN IF NOT EXISTS NotifyChannel INT NULL;
