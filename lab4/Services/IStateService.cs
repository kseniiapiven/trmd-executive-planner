namespace ExecutivePlanner.Services;

public interface IStateService
{
    string Get(string key, string defaultValue = "");
    void Set(string key, string value);
    void Remove(string key);
}
