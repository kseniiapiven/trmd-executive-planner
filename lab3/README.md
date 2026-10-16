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
