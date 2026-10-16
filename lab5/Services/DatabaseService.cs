using ExecutivePlanner.Helpers;
using ExecutivePlanner.Models;
using Microsoft.Extensions.Logging;
using SQLite;

namespace ExecutivePlanner.Services;

public class DatabaseService : IMeetingRepository
{
    private const SQLiteOpenFlags Flags =
        SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache;

    private readonly string _dbPath;

    private readonly ILogger<DatabaseService> _logger;
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private SQLiteAsyncConnection? _db;

    // dbPath задається в тестах (тимчасовий файл); за замовчуванням база лежить у каталозі даних застосунку.
    public DatabaseService(ILogger<DatabaseService> logger, string? dbPath = null)
    {
        _logger = logger;
        _dbPath = dbPath ?? Path.Combine(FileSystem.AppDataDirectory, "executiveplanner.db3");
    }

    // Ліниве створення з'єднання й таблиці за атрибутами моделі (code-first).
    private async Task<SQLiteAsyncConnection> GetConnectionAsync()
    {
        if (_db is not null) return _db;

        await _initLock.WaitAsync();
        try
        {
            if (_db is null)
            {
                var connection = new SQLiteAsyncConnection(_dbPath, Flags);
                await connection.CreateTableAsync<Meeting>();
                _db = connection;
                _logger.LogInformation("Базу даних відкрито: {Path}", _dbPath);
            }
        }
        finally
        {
            _initLock.Release();
        }

        return _db;
    }

    public Task<OperationResult<List<Meeting>>> GetAllAsync() =>
        RunAsync("GetAll", async db =>
        {
            var items = await db.Table<Meeting>().OrderBy(m => m.StartTime).ToListAsync();
            _logger.LogInformation("Прочитано зустрічей: {Count}", items.Count);
            return items;
        });

    public Task<OperationResult<Meeting?>> GetByIdAsync(int id) =>
        RunAsync<Meeting?>("GetById", async db =>
        {
            var meeting = await db.Table<Meeting>().FirstOrDefaultAsync(m => m.Id == id);
            if (meeting is null) _logger.LogWarning("Зустріч з Id={Id} відсутня в базі", id);
            return meeting;
        });

    public async Task<bool> InsertAsync(Meeting meeting) =>
        await RunBoolAsync("Insert", async db =>
        {
#if DEBUG
            // Тестовий збій для кроку 8 лабораторної роботи (лише в Debug-збірці).
            if (meeting.Title == "#test-error") throw new InvalidOperationException("Змодельований збій бази даних");
#endif
            var rows = await db.InsertAsync(meeting);
            _logger.LogInformation("Додано зустріч Id={Id}", meeting.Id);
            return rows > 0;
        });

    public async Task<bool> UpdateAsync(Meeting meeting) =>
        await RunBoolAsync("Update", async db =>
        {
            var rows = await db.UpdateAsync(meeting);
            if (rows == 0) _logger.LogWarning("Оновлення Id={Id}: рядок не знайдено", meeting.Id);
            else _logger.LogInformation("Оновлено зустріч Id={Id}", meeting.Id);
            return rows > 0;
        });

    public async Task<bool> DeleteAsync(Meeting meeting) =>
        await RunBoolAsync("Delete", async db =>
        {
            var rows = await db.DeleteAsync(meeting);
            if (rows == 0) _logger.LogWarning("Видалення Id={Id}: рядок не знайдено", meeting.Id);
            else _logger.LogInformation("Видалено зустріч Id={Id}", meeting.Id);
            return rows > 0;
        });

    // Для операцій запису: true лише коли виняток не виник І операція справді змінила рядок.
    private async Task<bool> RunBoolAsync(string operation, Func<SQLiteAsyncConnection, Task<bool>> action)
    {
        var result = await RunAsync(operation, action);
        return result.Success && result.Value;
    }

    // Єдина точка обробки передбачуваних помилок: виняток потрапляє в лог із контекстом операції,
    // а викликач отримує контрольований результат замість поширення винятку.
    private async Task<OperationResult<T>> RunAsync<T>(string operation, Func<SQLiteAsyncConnection, Task<T>> action)
    {
        try
        {
            var db = await GetConnectionAsync();
            return OperationResult<T>.Ok(await action(db));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка операції {Operation} над таблицею Meetings", operation);
            return OperationResult<T>.Fail();
        }
    }
}
