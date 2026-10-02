-- Добавляет AudienceTag (целевая аудитория) на Products — заполняется партнёрами при добавлении товара через свой кабинет.
ALTER TABLE Products ADD COLUMN IF NOT EXISTS AudienceTag VARCHAR(50) NULL;
