# Журнал взаємодії з LLM-асистентом (Claude)

| # | Запит | Відповідь (стисло) | Рішення студента та обґрунтування |
|---|-------|--------------------|-----------------------------------|
| 1 | Що потрібно встановити на macOS без Xcode для .NET MAUI і як налаштувати емулятор? | .NET 8 SDK, workload `maui-android`, JDK 17, Android SDK + AVD (arm64); iOS/Mac цілі потребують Xcode. | Обрано лише Android-ціль, як вимагає завдання; Xcode (~15 ГБ) не потрібен. |
| 2 | Згенерувати шаблон ViewModel для планувальника зустрічей | `MeetingsViewModel : ObservableObject` з `[ObservableProperty]`, `ObservableCollection<Meeting>`, `[RelayCommand] AddMeeting` із `CanExecute` (непорожня назва). | Прийнято source-generators замість ручного `INotifyPropertyChanged` — менше шаблонного коду; додано валідацію назви. |
| 3 | Як підключити ViewModel до View через DI і Shell | Зареєструвати ViewModel і Page у `MauiProgram`, у `AppShell` використати `ContentTemplate="{DataTemplate views:MeetingsPage}"`, `BindingContext` у конструкторі. | Прийнято; типізовані прив'язки `x:DataType` для перевірки на етапі компіляції. |
