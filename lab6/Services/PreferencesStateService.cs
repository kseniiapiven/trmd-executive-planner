namespace ExecutivePlanner.Services;

// Легковагове сховище «ключ-значення» на основі Preferences API (персистентне між запусками).
public class PreferencesStateService : IStateService
{
    public string Get(string key, string defaultValue = "") => Preferences.Default.Get(key, defaultValue);
    public void Set(string key, string value) => Preferences.Default.Set(key, value);
    public void Remove(string key) => Preferences.Default.Remove(key);
}
