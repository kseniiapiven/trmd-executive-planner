using ExecutivePlanner.Models;
using Microsoft.Extensions.Logging;
using SQLite;

namespace ExecutivePlanner.Services;

public class DatabaseService : IMeetingRepository
{
    private const SQLiteOpenFlags Flags =
        SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache;

    private static string DbPath => Path.Combine(FileSystem.AppDataDirectory, "executiveplanner.db3");

    private readonly ILogger<DatabaseService> _logger;
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private SQLiteAsyncConnection? _db;

    public DatabaseService(ILogger<DatabaseService> logger)
    {
        _logger = logger;
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
                var connection = new SQLiteAsyncConnection(DbPath, Flags);
                await connection.CreateTableAsync<Meeting>();
                _db = connection;
                _logger.LogInformation("Базу даних відкрито: {Path}", DbPath);
            }
        }
        finally
        {
            _initLock.Release();
        }

        return _db;
    }

    public async Task<List<Meeting>> GetAllAsync()
    {
        var db = await GetConnectionAsync();
        return await db.Table<Meeting>().OrderBy(m => m.StartTime).ToListAsync();
    }

    public async Task<Meeting?> GetByIdAsync(int id)
    {
        var db = await GetConnectionAsync();
        return await db.Table<Meeting>().FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<int> InsertAsync(Meeting meeting)
    {
        var db = await GetConnectionAsync();
        return await db.InsertAsync(meeting);
    }

    public async Task<int> UpdateAsync(Meeting meeting)
    {
        var db = await GetConnectionAsync();
        return await db.UpdateAsync(meeting);
    }

    public async Task<int> DeleteAsync(Meeting meeting)
    {
        var db = await GetConnectionAsync();
        return await db.DeleteAsync(meeting);
    }
}
