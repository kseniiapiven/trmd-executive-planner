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
