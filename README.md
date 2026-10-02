# FULL SELLER — Backend API

ASP.NET Core Web API (.NET 8) для мобильного приложения FULL SELLER.
Архитектура: **Domain / Infrastructure / WebApi**, доступ к данным — чистый **ADO.NET** (`Microsoft.Data.SqlClient`), без EF Core и без Dapper — как и требовалось.

## Структура решения

```
FullSeller.sln
├── src/FullSeller.Domain          — сущности, enum'ы, интерфейсы репозиториев/сервисов, исключения (без внешних зависимостей)
├── src/FullSeller.Infrastructure  — ADO.NET-репозитории, JWT/Telegram/SMS/файлы, SQL-миграции
└── src/FullSeller.WebApi          — контроллеры, DTO, аутентификация, Program.cs
```

Подробное описание классов и эндпоинтов — в документе `FULL_SELLER_TZ_Backend.docx`, который я присылал ранее. Код в этом решении реализует именно то ТЗ.

## Важно: NuGet в песочнице был недоступен

Пока я писал код в облачной песочнице, доступ к `api.nuget.org` был заблокирован политикой сети (проверял — не только NuGet, вообще любые pkg-реестры кроме npm/pypi). Из-за этого я не смог сам скомпилировать **Infrastructure** и **WebApi** (у них есть NuGet-пакеты: `Microsoft.Data.SqlClient`, JWT, Swagger).

Что я всё-таки проверил в песочнице:
- `FullSeller.Domain` — собирается без единой ошибки/предупреждения (`dotnet build` → `Build succeeded, 0 Warning(s), 0 Error(s)`), у него вообще нет NuGet-зависимостей.
- Весь остальной код написан вручную, аккуратно, с проверкой типов/сигнатур друг против друга (я много раз перечитывал Domain-интерфейсы, чтобы Infrastructure/WebApi точно на них ложились). Но финальную сборку `Infrastructure`/`WebApi` нужно один раз запустить у тебя — там обычный интернет, NuGet восстановится сам.

**На твоём компьютере просто:**

```bash
cd full-seller-back
dotnet restore
dotnet build
```

Если что-то не соберётся — пришли мне текст ошибки, поправлю сразу.

## Настройка перед первым запуском

1. **Строка подключения к MS SQL Server** — в `src/FullSeller.WebApi/appsettings.json`, секция `ConnectionStrings:FullSellerDb`. Замени `Server`, `Database`, `User Id`, `Password` на свои (или подними локально SQL Server / Docker-контейнер `mcr.microsoft.com/mssql/server`).
2. **JWT-секрет** — `Jwt:Key` в том же файле. Сейчас там заглушка `CHANGE_ME_...` — обязательно замени на длинную случайную строку перед реальным использованием (в проде — лучше вынести в переменные окружения / `dotnet user-secrets`, а не хранить в файле).
3. **Telegram Login Widget** (если будете использовать вход через Telegram) — `Telegram:BotToken`.
4. Миграции применяются **автоматически при старте** (`Database:MigrateOnStartup: true` в конфиге) — сам создаст таблицы при первом запуске. SQL-файл миграции: `src/FullSeller.Infrastructure/Data/Migrations/0001_init.sql` (20 таблиц + тарифы карго-доставки).

## Запуск

```bash
dotnet run --project src/FullSeller.WebApi
```

Swagger откроется на `https://localhost:xxxx/swagger` (порт — в `src/FullSeller.WebApi/Properties/launchSettings.json`). Там же можно авторизоваться через кнопку **Authorize**, вставив `Bearer {access_token}` — токен получаешь через `POST /api/auth/otp/confirm`.

## Что реализовано

- **Auth**: вход по SMS-коду (OTP) и через Telegram Login Widget (с реальной проверкой HMAC-подписи по алгоритму Telegram), JWT access+refresh токены.
- **Catalog**: категории, бренды, поиск/фильтрация товаров, карточка товара с оптовыми ценовыми планками (price tiers) и вариантами (размер/цвет).
- **Cart**: корзина, массовая загрузка позиций по артикулам (для оптовых клиентов).
- **Orders**: создание заказа из корзины одной SQL-транзакцией (проверка остатков, расчёт цены по объёму закупки, списание стока, расчёт доставки, минимальная сумма заказа), история статусов, повтор заказа, смена статуса менеджером.
- **Reviews**: отзывы только после доставленного заказа с этим товаром, очередь модерации для менеджера/админа.
- **Favorites, Chat, Notifications, Profile/Addresses** — полностью рабочие CRUD-эндпоинты.
- **Роли**: Customer / LegalCustomer / Manager / Admin — через кастомный claim `role` в JWT; менеджерские эндпоинты проверяются вручную (`User.IsManagerOrAdmin()`), т.к. роль не стандартная ASP.NET-роль.

### Добавлено под ТЗ FULL SELLER 2.0 (редизайн + недостающие функции админки)

- **AppSettings** (`/api/settings`) — курс USD→RUB (для переключателя валюты ₽ / $), минимальная сумма заказа,
  контакты и ссылки на WhatsApp/Telegram. Чтение — публично, изменение — Admin/SuperAdmin.
- **Banners** (`/api/banners`) — рекламные баннеры главной страницы, с сортировкой и флагом активности; публично отдаются
  только активные, полный список и CRUD — в админке.
- **PromoCodes** (`/api/promocodes`) — промокоды (процент/фиксированная сумма, срок действия, лимит использований),
  таблица в БД существовала, но не была подключена — теперь есть репозиторий, валидация с расчётом скидки и админский CRUD.
- **Бейджи товара**: `Product.OldPrice` / `IsNew` / `IsHit` — управляются вручную из админки, используются на клиенте
  для бейджей «Новинка» / «Хит» / «Скидка» и зачёркнутой старой цены.
- **Admin Catalog CRUD** (`/api/admin/catalog/**`) — раньше `CatalogController` был только на чтение. Добавлено полное
  управление категориями, брендами, товарами (включая скрытие товара вместо удаления — `SetProductActiveAsync`),
  вариантами (размер/цвет/сток) и оптовой шкалой цен (`ReplacePriceTiersAsync` пересчитывает `Products.MinPrice`).
- **Cargo rates admin** (`GET/PUT /api/delivery/rates`) — раньше тарифы карго можно было только читать через калькулятор;
  теперь админ может смотреть и менять цену за кг по стране (создаёт тариф, если его ещё нет).
- Миграция `0002_settings_banners_product_badges.sql` — добавляет колонки `Products.OldPrice/IsNew/IsHit`,
  таблицы `AppSettings` (с сид-строкой по умолчанию) и `Banners`.

## Что нужно заменить перед продакшеном (сделано как чистые заглушки за интерфейсом — меняется без переписывания остального кода)

- `ConsoleSmsSender` (`ISmsSender`) — сейчас просто пишет код в консоль, вместо реальной отправки SMS.
- `FirebasePushNotifier` (`IPushNotifier`) — заглушка, вместо реальной интеграции с FCM.
- `LocalFileStorage` (`IFileStorage`) — сохраняет файлы на диск сервера в `wwwroot/uploads`; для прода стоит заменить на S3-совместимое хранилище.

Всё остальное — рабочая бизнес-логика, не заглушки.
