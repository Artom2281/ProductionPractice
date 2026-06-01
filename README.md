# LibraryManagement

<div align="center">

Система управления библиотекой — десктопное приложение на **C# / .NET 10 / WinForms** с чистой многослойной архитектурой и **Entity Framework Core (SQLite)**.

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Platform](https://img.shields.io/badge/platform-Windows-0078D6?logo=windows&logoColor=white)](https://www.microsoft.com/windows)
[![UI](https://img.shields.io/badge/UI-WinForms-blueviolet)](https://learn.microsoft.com/dotnet/desktop/winforms/)
[![EF Core](https://img.shields.io/badge/EF%20Core-10.0.7-6E4CB8)](https://learn.microsoft.com/ef/core)
[![Database](https://img.shields.io/badge/DB-SQLite-003B57?logo=sqlite&logoColor=white)](https://www.sqlite.org/)
[![Validation](https://img.shields.io/badge/FluentValidation-12.1.1-1E88E5)](https://docs.fluentvalidation.net/)
[![License](https://img.shields.io/badge/license-MIT-green)](LICENSE)

</div>

---

## 📖 О проекте

**LibraryManagement** — учебный/производственно-практический проект автоматизации работы библиотеки: учёт книг, читателей, авторов, жанров, издательств, выдача и возврат книг, формирование отчётов и администрирование пользователей с разграничением ролей.

### Ключевые возможности

- 📚 Управление каталогом книг (CRUD)
- 👤 Учёт читателей и контактных данных
- ✍️ Учёт авторов, жанров и издательств
- 🔄 Выдача / возврат книг с отслеживанием статуса займа
- 📊 Отчёты (просроченные выдачи, активные читатели и т.п.)
- 🔐 Авторизация с ролевой моделью (Admin / Librarian / Reader)
- ✅ Валидация входных данных через FluentValidation
- 🗄️ Локальное хранилище SQLite — не требует отдельного сервера БД

---

## 🏗️ Архитектура

Решение построено по принципам **Clean Architecture** — зависимости направлены строго к Domain:

```
src/
├── LibraryManagement.Domain        ← Сущности, перечисления, доменные правила
├── LibraryManagement.Application   ← Сценарии использования, валидаторы, интерфейсы сервисов
├── LibraryManagement.Data          ← EF Core: DbContext, миграции, конфигурации сущностей
└── LibraryManagement.WinForms      ← UI: формы, контролы, презентеры, DI-композиция
```

| Слой | Назначение |
|---|---|
| **Domain** | `Author`, `Book`, `Genre`, `Loan`, `Publisher`, `Reader`, `User`; `LoanStatus`, `UserRole` |
| **Application** | Валидаторы FluentValidation, контракты сервисов, бизнес-сценарии |
| **Data** | `LibraryDbContext`, миграции EF Core, конфигурации маппинга сущностей |
| **WinForms** | `MainForm` + набор форм по сущностям: `Books`, `Readers`, `Authors`, `Loans`, `Publishers`, `Genres`, `Reports`, `Admin`, `Auth` |

### Структура UI

```
LibraryManagement.WinForms/
├── Forms/
│   ├── MainForm / MainFormPresenter
│   ├── Auth/         (Login, Register)
│   ├── Admin/        (Users)
│   ├── Books/        (BooksList, BookEdit)
│   ├── Readers/      (ReadersList, ReaderEdit)
│   ├── Authors/      (AuthorsList, AuthorEdit)
│   ├── Genres/       (GenresList, GenreEdit)
│   ├── Publishers/   (PublishersList, PublisherEdit)
│   ├── Loans/        (IssueLoan, LoansList, ReturnLoan)
│   └── Reports/
├── Controls/         (CrudListControl, NavigationControl, SimpleGrid…)
├── Services/         (AppDbContextFactory, AppHost, CurrentUser…)
└── Helpers/          (Ui-утилиты)
```

---

## 🛠️ Стек

- **Язык:** C# 13
- **Платформа:** .NET 10 (`net10.0` / `net10.0-windows` для WinForms)
- **UI:** Windows Forms
- **ORM:** Entity Framework Core 10.0.7
- **БД:** SQLite (файл `library.db` создаётся автоматически)
- **Валидация:** FluentValidation 12.1.1
- **DI / Configuration:** `Microsoft.Extensions.Hosting` + `Microsoft.Extensions.Configuration.Json`

---

## 🚀 Быстрый старт

### Требования

- Windows 10/11
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Visual Studio 2026 / Rider 2025+ (опционально, можно собирать из CLI)

### Сборка и запуск из CLI

```powershell
# Клонирование
git clone https://github.com/<owner>/LibraryManagement.git
cd LibraryManagement

# Восстановление зависимостей
dotnet restore

# Сборка
dotnet build

# Запуск приложения
dotnet run --project src/LibraryManagement.WinForms
```

При первом запуске:
1. В каталоге `src/LibraryManagement.WinForms/bin/<Configuration>/net10.0-windows/` будет создан файл `library.db`.
2. Примените миграции для создания схемы БД:

   ```powershell
   dotnet ef database update --project src/LibraryManagement.Data --startup-project src/LibraryManagement.WinForms
   ```

3. Зарегистрируйте первого пользователя (по умолчанию — через форму `Register`).

### Строка подключения

Файл `src/LibraryManagement.WinForms/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "Library": "Data Source=library.db"
  }
}
```

> ⚠️ Файл `library.db` добавлен в `.gitignore` — это локальная БД разработчика, она не должна попадать в репозиторий.

---

## 🖼️ Скриншоты

> Скриншоты UI будут добавлены позднее автором. Структура каталога:
>
> ```
> docs/screenshots/
> ├── auth-login.png
> ├── main.png
> ├── books-list.png
> ├── books-edit.png
> ├── readers.png
> ├── loans.png
> └── reports.png
> ```

---

## 🧱 Миграции БД

Создание новой миграции после изменения моделей:

```powershell
dotnet ef migrations add <MigrationName> `
  --project src/LibraryManagement.Data `
  --startup-project src/LibraryManagement.WinForms
```

Применение миграций:

```powershell
dotnet ef database update `
  --project src/LibraryManagement.Data `
  --startup-project src/LibraryManagement.WinForms
```

---

## 🧪 Тестирование

В репозитории пока не выделен отдельный test-проект. Рекомендуемая структура для будущего расширения:

```
tests/
├── LibraryManagement.Domain.Tests
├── LibraryManagement.Application.Tests
└── LibraryManagement.IntegrationTests
```

Запуск (после добавления):

```powershell
dotnet test
```

---

## 📐 Соглашения по коду

- **Язык интерфейса:** русский
- **Именование в коде:** английский (стандартные convention C#)
- **Nullable reference types:** включены
- **Implicit usings:** включены
- Комментарии — обычные `//` или `/* */`, без декоративных разделителей
- Архитектурные правила проекта зафиксированы в [`CLAUDE.md`](CLAUDE.md)

---

## 🤝 Вклад

1. Форкните репозиторий
2. Создайте feature-ветку: `git checkout -b feature/amazing-feature`
3. Сделайте коммит: `git commit -m "feat: add amazing feature"`
4. Запушьте ветку: `git push origin feature/amazing-feature`
5. Откройте Pull Request

---

## 📄 Лицензия

Распространяется под лицензией **MIT**. См. [`LICENSE`](LICENSE).

---

## 👤 Автор

**Popov** — производственная практика, 2026.

<div align="center">

⭐ Если проект был полезен — поставьте звезду на GitHub!

</div>
