-- Пароль пользователя (PBKDF2-хеш) для входа по номеру + паролю.
ALTER TABLE Users ADD COLUMN IF NOT EXISTS PasswordHash VARCHAR(256) NULL;

-- Приводим номера к единому виду "+992XXXXXXXXX" (без пробелов/скобок/дефисов),
-- чтобы вход по паролю находил аккаунт независимо от того, как номер был введён.
UPDATE Users u
SET Phone = '+' || regexp_replace(u.Phone, '\D', '', 'g')
WHERE u.Phone NOT LIKE 'tg:%'
  AND u.Phone <> '+' || regexp_replace(u.Phone, '\D', '', 'g')
  AND NOT EXISTS (
      SELECT 1 FROM Users x WHERE x.Phone = '+' || regexp_replace(u.Phone, '\D', '', 'g')
  );
