using SQLite;

namespace ExecutivePlanner.Models;

[Table("Meetings")]
public class Meeting
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [NotNull, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Location { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

    public DateTime StartTime { get; set; }

    // Фото (шлях до файлу у сховищі застосунку) і координати місця зустрічі
    public string ImagePath { get; set; } = string.Empty;

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    [Ignore]
    public string StartDisplay => StartTime.ToString("dd.MM.yyyy HH:mm");
}
