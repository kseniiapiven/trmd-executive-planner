using ExecutivePlanner.Models;
using ExecutivePlanner.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace ExecutivePlanner.Tests.Integration;

// Інтеграційні тести: реальний SQLite у тимчасовому файлі, який видаляється після кожного тесту.
public class DatabaseServiceTests : IDisposable
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"planner-test-{Guid.NewGuid():N}.db3");
    private readonly DatabaseService _service;

    public DatabaseServiceTests()
    {
        _service = new DatabaseService(NullLogger<DatabaseService>.Instance, _dbPath);
    }

    public void Dispose()
    {
        foreach (var path in new[] { _dbPath, _dbPath + "-wal", _dbPath + "-shm" })
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    private static Meeting NewMeeting(string title, int hour = 10) => new()
    {
        Title = title,
        Location = "Офіс",
        Notes = "нотатки",
        StartTime = new DateTime(2026, 10, 16, hour, 0, 0)
    };

    [Fact]
    public async Task Crud_Sequence_InsertGetAllUpdateDelete_WorksOnRealDatabase()
    {
        // Arrange
        var meeting = NewMeeting("Нарада");

        // Act + Assert: Insert
        Assert.True(await _service.InsertAsync(meeting));
        Assert.True(meeting.Id > 0, "Id має бути згенерований базою (AutoIncrement)");

        // GetAll
        var all = await _service.GetAllAsync();
        Assert.True(all.Success);
        var stored = Assert.Single(all.Value!);
        Assert.Equal("Нарада", stored.Title);

        // Update
        stored.Title = "Нарада (перенесена)";
        Assert.True(await _service.UpdateAsync(stored));
        var updated = await _service.GetByIdAsync(stored.Id);
        Assert.Equal("Нарада (перенесена)", updated.Value!.Title);

        // Delete
        Assert.True(await _service.DeleteAsync(stored));
        var afterDelete = await _service.GetAllAsync();
        Assert.Empty(afterDelete.Value!);
    }

    [Fact]
    public async Task GetById_WhenMeetingMissing_ReturnsSuccessWithNull()
    {
        var result = await _service.GetByIdAsync(999);

        Assert.True(result.Success);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task GetAll_ReturnsMeetingsOrderedByStartTime()
    {
        // Arrange
        await _service.InsertAsync(NewMeeting("Пізня", hour: 16));
        await _service.InsertAsync(NewMeeting("Рання", hour: 8));
        await _service.InsertAsync(NewMeeting("Середня", hour: 12));

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.Equal(new[] { "Рання", "Середня", "Пізня" }, result.Value!.Select(m => m.Title));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    public async Task GetAll_ReturnsAsManyMeetingsAsWereInserted(int count)
    {
        // Arrange
        for (var i = 0; i < count; i++)
            await _service.InsertAsync(NewMeeting($"Зустріч {i}"));

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.Equal(count, result.Value!.Count);
    }

    [Fact]
    public async Task Insert_WithNullTitle_ViolatesNotNullConstraintAndReturnsFalse()
    {
        // Arrange: Title має обмеження NOT NULL
        var invalid = NewMeeting("x");
        invalid.Title = null!;

        // Act
        var inserted = await _service.InsertAsync(invalid);

        // Assert: збій оброблено контрольовано, виняток не поширився, у базу нічого не потрапило
        Assert.False(inserted);
        Assert.Empty((await _service.GetAllAsync()).Value!);
    }

    [Fact]
    public async Task Update_NonExistingMeeting_ReturnsFalse()
    {
        var ghost = NewMeeting("Привид");
        ghost.Id = 12345;

        Assert.False(await _service.UpdateAsync(ghost));
    }

    [Fact]
    public async Task Delete_NonExistingMeeting_ReturnsFalse()
    {
        var ghost = NewMeeting("Привид");
        ghost.Id = 12345;

        Assert.False(await _service.DeleteAsync(ghost));
    }

    [Fact]
    public async Task Data_SurvivesCreatingNewServiceOnSameFile()
    {
        // Arrange
        await _service.InsertAsync(NewMeeting("Збережена"));

        // Act: новий екземпляр сервісу відкриває той самий файл (імітація перезапуску застосунку)
        var reopened = new DatabaseService(NullLogger<DatabaseService>.Instance, _dbPath);
        var result = await reopened.GetAllAsync();

        // Assert
        Assert.Equal("Збережена", Assert.Single(result.Value!).Title);
    }
}
