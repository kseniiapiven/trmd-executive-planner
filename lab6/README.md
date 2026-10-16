# ExecutivePlanner

Лабораторна робота №1 (ТРМД, магістратура). Варіант 10: планувальник ділових зустрічей і завдань керівника.
Кросплатформний застосунок на .NET 8 / .NET MAUI за патерном MVVM (CommunityToolkit.Mvvm).

## Структура
- `Models/` — сутність `Meeting` (Title, Location, StartTime)
- `ViewModels/` — `MeetingsViewModel` (ObservableObject, колекція в пам'яті, `AddMeetingCommand`)
- `Views/` — `MeetingsPage` (CollectionView, форма, кнопка)
- `Services/`, `Helpers/` — зарезервовано для наступних робіт
- DI: `MauiProgram.cs` реєструє ViewModel і View; `BindingContext` задається через конструктор сторінки.

## Запуск (Android)
```
dotnet build -f net8.0-android -t:Run
```
Знімки екрана: `docs/`. Журнал звернень до LLM: `docs/llm-log.md`.

## Лабораторна робота №2: життєвий цикл, навігація, стан
- **Другий екран** `MeetingDetailPage` / `MeetingDetailViewModel` — деталі й редагування зустрічі.
- **Навігація Shell**: маршрут реєструється в `AppShell.xaml.cs`; перехід `GoToAsync("MeetingDetailPage?id=<Guid>")` за вибором елемента списку (`OpenMeetingCommand`); параметр приймає `IQueryAttributable` у `MeetingDetailViewModel`.
- **Життєвий цикл**: інтерфейс `Helpers/ILifecycleAware`; code-behind сторінок лише делегує `OnAppearing/OnDisappearing` у ViewModel. Події `App` (OnStart/OnSleep/OnResume) логуються через `Debug.WriteLine`.
- **Стан**: `Services/PreferencesStateService` (Preferences API); чернетка форми зберігається в `App.OnSleep` та `OnDisappearing`, відновлюється при старті.
- Спільні дані двох екранів — `IMeetingService` (в пам'яті; у ЛР3 буде SQLite).
- Знімки та спостережений лог: `docs/lab2/`.

## Лабораторна робота №3: SQLite і патерн Repository
- Пакет `sqlite-net-pcl`; файл бази `executiveplanner.db3` у `FileSystem.AppDataDirectory`.
- Схема (code-first, атрибути в `Models/Meeting.cs`), таблиця `Meetings`:

| Поле | Тип | Обмеження |
|---|---|---|
| Id | INTEGER | PRIMARY KEY, AUTOINCREMENT |
| Title | VARCHAR(200) | NOT NULL |
| Location | VARCHAR(200) | |
| Notes | VARCHAR | |
| StartTime | BIGINT (ticks) | |

- `Services/IMeetingRepository` - контракт; `Services/DatabaseService` - реалізація (асинхронні GetAllAsync, GetByIdAsync, InsertAsync, UpdateAsync, DeleteAsync), реєструється в DI як Singleton.
- ViewModel працюють лише з `IMeetingRepository`; список перезавантажується в `OnAppearing`; на екрані деталей додано команду видалення.
- Знімки екрана і вміст бази: `docs/lab3/`.

## Лабораторна робота №4: логування та обробка помилок
- **Логування**: `Microsoft.Extensions.Logging`; провайдери Debug (Output) та власний `Helpers/FileLoggerProvider` (файл `executiveplanner.log` у `FileSystem.AppDataDirectory`, формат `час [РІВЕНЬ] категорія: повідомлення`). `ILogger<T>` впроваджується в `DatabaseService` та ViewModel; мінімальний рівень - Debug.
- **Рівні**: Debug - життєвий цикл і стан; Information - успішні операції (завантаження, додавання, оновлення, видалення); Warning - запис не знайдено; Error (FAIL у файлі) - оброблений виняток у Repository; Critical - необроблений виняток на рівні застосунку.
- **Repository**: методи `DatabaseService` обгорнуті в `try-catch` (`RunAsync`), повертають `OperationResult<T>` або `bool`; виняток не поширюється, а потрапляє в лог з контекстом.
- **Користувач**: ViewModel показує через `AlertHelper` лише зрозуміле повідомлення, без технічних деталей.
- **Глобальний обробник**: `AppDomain.UnhandledException` та `TaskScheduler.UnobservedTaskException` у `MauiProgram.cs` пишуть `LogCritical`.
- **Тестові збої (лише Debug)**: назва зустрічі `#test-error` викликає збій у Repository, `#test-unobserved` - неспостережений виняток у фоновій задачі.
- Знімки екрана та приклад логу: `docs/lab4/`.

## Лабораторна робота №5: тестування
- Тестовий проєкт `Tests/ExecutivePlanner.Tests` (xUnit + Moq), підключений до рішення `ExecutivePlanner.sln`. Для цього застосунок додатково збирається під `net8.0` (лише для тестів; запускається він на Android).
- **Запуск тестів**: `dotnet test Tests/ExecutivePlanner.Tests`
- **Статистика**: 34 тести - 24 модульних (`Unit/`: ViewModel з mock-репозиторієм, діалогами й навігацією, патерн AAA, 2 параметризовані тести) і 10 інтеграційних (`Integration/`: реальний SQLite у тимчасовому файлі, який видаляється після тесту).
- Для тестованості додано інтерфейси `IDialogService` та `INavigationService`; `DatabaseService` приймає шлях до файлу бази.
- **Дефекти, знайдені тестами**: (1) ViewModel не перехоплювала винятки репозиторію - виправлено; (2) `DatabaseService` повертав «успіх» для оновлення й видалення неіснуючого запису - виправлено.
- Логи запуску тестів до й після виправлення та їх знімки: `docs/lab5/`.

## Лабораторна робота №6: ролі, камера, геолокація
- **Користувачі й ролі**: таблиця `Users` (Id, Login, PasswordHash, Role); ролі `Manager` (Керівник) і `Assistant` (Помічник). Пароль зберігається лише як солоний хеш PBKDF2-SHA256 (`Helpers/PasswordHasher`).
- **Автентифікація**: `AuthService` (`IAuthService`), екран входу `LoginPage`, спільна сесія в `AuthService.CurrentUser`.
- **Демонстраційні облікові записи** (створюються при першому запуску в порожній базі): `manager` / `manager123` (Керівник) та `assistant` / `assistant123` (Помічник).
- **Розмежування доступу**: видалення зустрічі дозволене лише Керівнику - кнопка «Видалити» прихована для Помічника, а команда `Delete` у ViewModel додатково перевіряє роль.
- **Камера і геолокація**: `ICameraService` (`MediaPicker`) та `IGeolocationService` (`Geolocation`) - окремі сервіси в DI; перед зверненням перевіряється й запитується дозвіл (`Permissions`); відмова логується (Warning) і показується користувачу зрозуміле повідомлення. Фото зберігається в `FileSystem.AppDataDirectory/photos`, шлях і координати - у полях `ImagePath`, `Latitude`, `Longitude` моделі `Meeting`.
- **Дозволи Android**: `CAMERA`, `ACCESS_FINE_LOCATION`, `ACCESS_COARSE_LOCATION` у `Platforms/Android/AndroidManifest.xml`.
- **Тести**: 79 (64 модульних і 15 інтеграційних): `dotnet test Tests/ExecutivePlanner.Tests`.
- Знімки екрана і фрагмент логу: `docs/lab6/`.

## Підсумок: функціонал застосунку (ЛР1-6)
Планувальник ділових зустрічей керівника на .NET MAUI (Android): вхід за роллю; список зустрічей у SQLite; додавання, редагування й (для Керівника) видалення; фото й координати місця зустрічі; збереження чернетки форми між сеансами; навігація Shell з параметрами; файлове логування й глобальна обробка помилок; 79 автоматичних тестів. Архітектура: MVVM (CommunityToolkit.Mvvm) + Repository + сервіси за інтерфейсами в DI.
